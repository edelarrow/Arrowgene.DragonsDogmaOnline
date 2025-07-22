using System.Collections.Generic;

public class ChatCommand : IChatCommand
{
    public override AccountStateType AccountState => AccountStateType.User;
    public override string CommandName => "fashionload";
    public override string HelpText => "usage: `/fashionload password (PawnName)` - Loads a stored fashion template.";

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

        if (command.Length == 0)
        {
            responses.Add(ChatResponse.CommandError(client, "No password provided."));
            return;
        }

        string password = command[0];

        if (command.Length >= 2)
        {
            var tuple = client.Character.Pawns
                .Select((pawn, index) => new { pawn = pawn, pawnNumber = (byte)(index + 1) })
                .Where(tuple => tuple.pawn.Name == command[1])
                .FirstOrDefault();

            if (tuple == null)
            {
                responses.Add(ChatResponse.CommandError(client, "No pawn was found by that name."));
                return;
            }

            targetCharacter = tuple.pawn;
        }

        if (server.FashionManager.SetFromLockedFashion(client, targetCharacter, password))
        {
            responses.Add(ChatResponse.ServerChat(client, $"Template for {targetCharacter.CDataCharacterName}:"));
            foreach (var name in server.FashionManager.GetNames(client, targetCharacter))
            {
                responses.Add(ChatResponse.ServerChat(client, $"{name}"));
            }
        }
        else
        {
            responses.Add(ChatResponse.CommandError(client, $"No template was found by that name."));
        }
        
    }
}

return new ChatCommand();
