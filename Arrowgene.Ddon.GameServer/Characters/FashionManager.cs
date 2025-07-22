using Arrowgene.Ddon.GameServer.Party;
using Arrowgene.Ddon.Server;
using Arrowgene.Ddon.Server.Network;
using Arrowgene.Ddon.Shared.Entity.PacketStructure;
using Arrowgene.Ddon.Shared.Entity.Structure;
using Arrowgene.Ddon.Shared.Model;
using Arrowgene.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Arrowgene.Ddon.GameServer.Characters
{
    public class FashionManager(DdonGameServer server)
    {
        private static readonly ServerLogger Logger = LogProvider.Logger<ServerLogger>(typeof(FashionManager));

        private DdonGameServer Server = server;
        private ConditionalWeakTable<CharacterCommon, List<string>> FashionTable { get; } = [];
        private ConditionalWeakTable<GameClient, Dictionary<string, List<string>>> LockedFashions { get; } = [];

        private HashSet<StorageType> StorageTypes { get; } = [StorageType.StorageBoxNormal, StorageType.StorageBoxExpansion, StorageType.ItemBagEquipment];

        public static byte TOTAL_EQUIP_SLOTS => EquipmentTemplate.TOTAL_EQUIP_SLOTS;

        public List<string> GetFashionData(CharacterCommon character)
        {
            return FashionTable.GetValue(character, x => [.. Enumerable.Repeat<string>(null, TOTAL_EQUIP_SLOTS)]);
        }

        public void ResetFashionData(CharacterCommon character)
        {
            FashionTable.AddOrUpdate(character, [.. Enumerable.Repeat<string>(null, TOTAL_EQUIP_SLOTS)]);
        }

        public void SetFashionData(CharacterCommon character)
        {
            FashionTable.AddOrUpdate(character, FetchFashionFromCharacter(character));
        }

        public void AddFashionData(CharacterCommon character)
        {
            var currentFashion = GetFashionData(character);
            var newFashion = FetchFashionFromCharacter(character);

            for (int i = 0; i < TOTAL_EQUIP_SLOTS; i++)
            {
                if (newFashion[i] is not null)
                {
                    currentFashion[i] = newFashion[i];
                }
            }
        }

        public void LockFashion(GameClient client, CharacterCommon character, string password)
        {
            LockedFashions.GetValue(client, x => [])[password] = GetFashionData(character);
        }

        public bool SetFromLockedFashion(GameClient client, CharacterCommon character, string password)
        {
            if (TryGetLockedFashion(client, password, out var fashions))
            {
                FashionTable.AddOrUpdate(character, fashions);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool TryGetLockedFashion(GameClient client, string password, out List<string> fashions)
        {
            if (!LockedFashions.TryGetValue(client, out var dict))
            {
                fashions = null;
                return false;
            }
            else
            {
                var status = dict.TryGetValue(password, out var foundFashions);
                fashions = foundFashions;
                return status;
            }
        }

        public List<string> GetNames(GameClient client, CharacterCommon character)
        {
            List<string> strings = [];
            foreach(var item in GetFashionData(character))
            {
                if (item is null)
                {
                    continue;
                }

                var foundItem = client.Character.Storage.FindItemByUIdInStorage(ItemManager.EquipmentStorages, item);
                if (foundItem is null)
                {
                    strings.Add("* !UNKNOWN ITEM!");
                }
                else if (StorageTypes.Contains(foundItem.Item1))
                {
                    var itemData = Server.AssetRepository.ClientItemInfos[foundItem.Item2.Item2.ItemId];
                    strings.Add($"* {itemData.Name} -OK-");
                }
                else
                {
                    var itemData = Server.AssetRepository.ClientItemInfos[foundItem.Item2.Item2.ItemId];
                    strings.Add($"* {itemData.Name} !NOT AVAILABLE!");
                }
            }

            return strings;
        }

        public List<CDataCharacterEquipInfo> AsCDataCharacterEquipInfo(CharacterCommon character)
        {
            return [.. GetFashionData(character)
                .Select((x, index) => new { item = x, slot = (byte)(index + 1) })
                .Where(tuple => tuple.item != null)
                .Select(tuple => new CDataCharacterEquipInfo()
                {
                    EquipItemUId = tuple!.item,
                    EquipType = EquipType.Visual,
                    EquipCategory = tuple!.slot
                })];
        }

        public (bool Status, List<string> Names) CheckItems(GameClient client, CharacterCommon character)
        {
            bool check = true;
            foreach (var item in GetFashionData(character))
            {
                if (item is null || !check)
                {
                    continue;
                }

                check &= client.Character.Storage.FindItemByUIdInStorage(StorageTypes, item) is not null;
            }

            return (check, GetNames(client, character));
        }

        public PacketQueue HandleSwap(GameClient client, CharacterCommon character)
        {
            PacketQueue queue = new();

            Server.Database.ExecuteInTransaction(connection =>
            {
                queue.AddRange(Server.EquipManager.HandleChangeEquipList(
                    Server, client,
                    character,
                    AsCDataCharacterEquipInfo(character),
                    ItemNoticeType.ChangeEquip,
                    [.. StorageTypes],
                    connection));
            });

            if (character is Character arisen)
            {
                client.Enqueue(new S2CEquipChangeCharacterEquipNtc()
                {
                    CharacterId = arisen.CharacterId,
                    EquipItemList = character.Equipment.AsCDataEquipItemInfo(EquipType.Performance),
                    VisualEquipItemList = character.Equipment.AsCDataEquipItemInfo(EquipType.Visual)
                }, queue);

                client.Enqueue(arisen.S2CContextGetLobbyPlayerContextNtc, queue);
            }
            else if (character is Pawn pawn)
            {
                client.Enqueue(new S2CEquipChangePawnEquipNtc()
                {
                    CharacterId = pawn.CharacterId,
                    PawnId = pawn.PawnId,
                    EquipItemList = pawn.Equipment.AsCDataEquipItemInfo(EquipType.Performance),
                    VisualEquipItemList = pawn.Equipment.AsCDataEquipItemInfo(EquipType.Visual),
                }, queue);

                var member = client.Party.GetPartyMemberByCharacter(pawn);
                if (member is not null && member is PawnPartyMember pawnMember)
                {
                    client.Party.EnqueueToAll(pawnMember.GetPartyContext(), queue);
                }
            }


            return queue;
        }

        private List<string> FetchFashionFromCharacter(CharacterCommon character)
        {
            return new List<string>(character.EquipmentTemplate
                .GetEquipment(character.Job, EquipType.Visual)
                .Select(x => x?.UId))
            {
                // Trim out weapon and subweapon for animation reasons.
                [0] = null,
                [1] = null
            };
        }
    }
}
