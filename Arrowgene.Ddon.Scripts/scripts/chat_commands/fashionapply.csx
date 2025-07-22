using System.Collections.Generic;

public class ChatCommand : IChatCommand
{
    public override AccountStateType AccountState => AccountStateType.User;
    public override string CommandName => "fashionapply";
    public override string HelpText => "usage: `/fashionapply (PawnName)` - Apply the current fashion template.";

    public override void Execute(DdonGameServer server, string[] command, GameClient client, ChatMessage message, List<ChatResponse> responses)
    {
        if (!LibDdon.GetSetting<bool>("ChatCommandSettings", "EnableFashionCommands"))
        {
            responses.Add(ChatResponse.CommandError(client, "This command is not enabled."));
            return;
        }

        if (!client.Character.HasContentReleased(ContentsRelease.DressEquipment))
        {
            responses.Add(ChatResponse.CommandError(client, "You do not have this feature unlocked yet."));
            return;
        }

        CharacterCommon targetCharacter = client.Character;

        if (command.Length >= 1)
        {
            var tuple = client.Character.Pawns
                .Select((pawn, index) => new { pawn = pawn, pawnNumber = (byte)(index + 1) })
                .Where(tuple => tuple.pawn.Name == command[0])
                .FirstOrDefault();

            if (tuple == null)
            {
                responses.Add(ChatResponse.CommandError(client, "No pawn was found by that name."));
                return;
            }

            targetCharacter = tuple.pawn;
        }

        var (status, names) = server.FashionManager.CheckItems(client, targetCharacter);
        if (!status)
        {
            responses.Add(ChatResponse.CommandError(client, $"Template could not be applied; item missing."));
            foreach (var name in server.FashionManager.GetNames(client, targetCharacter))
            {
                responses.Add(ChatResponse.CommandError(client, $"{name}"));
            }
            return;
        }

        try
        {
            server.FashionManager.HandleSwap(client, targetCharacter).Send();
            responses.Add(ChatResponse.ServerChat(client, $"Template for {targetCharacter.CDataCharacterName} applied."));
        }
        catch (Exception ex)
        {
            responses.Add(ChatResponse.CommandError(client, $"Something went wrong. You may not have enough inventory space to handle the swapping. Relog to prevent item desyncs."));
        }
    }
}

return new ChatCommand();
