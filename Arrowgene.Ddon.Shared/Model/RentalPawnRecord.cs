using Arrowgene.Ddon.Shared.Entity.Structure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arrowgene.Ddon.Shared.Model
{
 
    /// <summary>
    /// IMPORTANT: If this class changes shape, the table `ddon_rental_pawn` needs to be emptied to prevent improperly shared records from trying to be deserialized.
    /// </summary>
    public class RentalPawnRecord
    {
        public uint PawnId { get; set; }
        public uint CharacterId { get; set; }
        public uint CommonId { get; set; }
        public string Name { get; set; }
        public List<CDataPawnReaction> PawnReactionList { get; set; } = [];
        public CDataPawnCraftData CraftData { get; set; } = new();
        public byte[] TrainingStatus { get; set; } = [];
        public List<CDataSpSkill> SpSkills { get; set; } = [];
        public bool IsOfficialPawn { get; set; }
        public CDataEditInfo EditInfo { get; set; } = new();
        public JobId Job { get; set; }
        public bool HideEquipHead { get; set; }
        public bool HideEquipLantern { get; set; }
        public CDataCharacterJobData CharacterJobData { get; set; } = new();
        public Equipment Equipment { get; set; }
        public List<CDataNormalSkillParam> LearnedNormalSkills { get; set; } = [];
        public List<CustomSkill?> EquippedCustomSkills { get; set; } = [];
        public List<Ability?> EquippedAbilities { get; set; } = [];
        public CDataOrbGainExtendParam ExtendedParams { get; set; } = new();
        public DateTime HireDate { get; set; }
        public CharacterProfile PawnProfile { get; set; } = new();

        public RentalPawnRecord()
        {
            // This has to exist for JSON reasons.
        }

        public static RentalPawnRecord FromPawn(Pawn pawn)
        {
            var record = new RentalPawnRecord()
            {
                PawnId = pawn.PawnId,
                CharacterId = pawn.CharacterId,
                CommonId = pawn.CommonId,
                Name = pawn.Name,
                PawnReactionList = pawn.PawnReactionList,
                CraftData = pawn.CraftData,
                TrainingStatus = pawn.TrainingStatus.GetValueOrDefault(pawn.Job, new byte[64]),
                SpSkills = pawn.SpSkills.GetValueOrDefault(pawn.Job, []),
                IsOfficialPawn = pawn.IsOfficialPawn,
                EditInfo = pawn.EditInfo,
                Job = pawn.Job,
                HideEquipHead = pawn.HideEquipHead,
                HideEquipLantern = pawn.HideEquipLantern,
                CharacterJobData = pawn.CharacterJobDataList.FirstOrDefault(x => x.Job == pawn.Job),
                Equipment = pawn.Equipment,
                LearnedNormalSkills = [.. pawn.LearnedNormalSkills.Where(x => x.Job == pawn.Job)],
                EquippedCustomSkills = pawn.EquippedCustomSkillsDictionary.GetValueOrDefault(pawn.Job),
                EquippedAbilities = pawn.EquippedAbilitiesDictionary.GetValueOrDefault(pawn.Job),
                ExtendedParams = pawn.ExtendedParams + pawn.ExtendedJobParams.GetValueOrDefault(pawn.Job, new()),
                HireDate = DateTime.Now,
                PawnProfile = pawn.CharacterProfile
            };

            return record;
        }

        public RentalPawn ToRentalPawn(uint hiringCharacterId, byte adventureCount, byte craftCount, uint killCount = 0)
        {
            RentalPawn pawn = new()
            {
                // RentalPawn Fields
                OwningCharacterId = this.CharacterId,
                AdventureCount = adventureCount,
                CraftCount = craftCount,
                KillCount = 0,
                HireDate = HireDate,

                // Pawn Fields
                PawnId = this.PawnId,
                CharacterId = hiringCharacterId,
                Name = this.Name,
                HmType = 1,
                PawnType = PawnType.Support,
                PawnReactionList = this.PawnReactionList,
                CraftData = this.CraftData,
                TrainingStatus = new() { { this.Job, this.TrainingStatus } },
                SpSkills = new() { { this.Job, this.SpSkills } },
                IsOfficialPawn = this.IsOfficialPawn,
                IsRented = true,
                PawnState = PawnState.None,

                // CharacterCommon Fields
                CommonId = this.CommonId,
                Server = new(), // ???
                EditInfo = this.EditInfo,
                StatusInfo = new(),
                Job = this.Job,
                HideEquipHead = this.HideEquipHead,
                HideEquipLantern = this.HideEquipLantern,
                CharacterJobDataList = new() { this.CharacterJobData },
                Equipment = this.Equipment,
                JewelrySlotNum = (byte)(1 + this.ExtendedParams.JewelrySlot),
                LearnedNormalSkills = this.LearnedNormalSkills,
                LearnedCustomSkills = [.. this.EquippedCustomSkills.Where(x => x is not null)],
                EquippedCustomSkillsDictionary = new() { { this.Job, this.EquippedCustomSkills } },
                LearnedAbilities = [.. this.EquippedAbilities.Where(x => x is not null)],
                EquippedAbilitiesDictionary = new() { { this.Job, this.EquippedAbilities } },
                ExtendedParams = this.ExtendedParams,
                ExtendedJobParams = new() { { this.Job, new() } },
                OrbRelease = new(),
                CharacterProfile = this.PawnProfile
            };

            pawn.StatusInfo.GainAttack = this.ExtendedParams.Attack;
            pawn.StatusInfo.GainDefense = this.ExtendedParams.Defence;
            pawn.StatusInfo.GainMagicAttack = this.ExtendedParams.MagicAttack;
            pawn.StatusInfo.GainMagicDefense = this.ExtendedParams.MagicDefence;
            pawn.StatusInfo.GainStamina = this.ExtendedParams.StaminaMax;
            pawn.StatusInfo.GainHP = this.ExtendedParams.HpMax;

            pawn.StatusInfo.MaxHP = 760U;
            pawn.StatusInfo.MaxStamina = 450U;
            pawn.StatusInfo.HP = uint.MaxValue;
            pawn.StatusInfo.WhiteHP = uint.MaxValue;
            pawn.StatusInfo.Stamina = uint.MaxValue;

            return pawn;
        }
    }
}
