public class ChatCommand : IChatCommand
{
    public override AccountStateType AccountState => AccountStateType.User;
    public override string CommandName => "fashionset";
    public override string HelpText => "usage: `/fashionset (PawnName)` - Sets your stored fashion template to what you're currently wearing.";

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

        server.FashionManager.SetFashionData(targetCharacter);

        responses.Add(ChatResponse.ServerChat(client, $"Template for {targetCharacter.CDataCharacterName}:"));
        foreach (var name in server.FashionManager.GetNames(client, targetCharacter))
        {
            responses.Add(ChatResponse.ServerChat(client, $"{name}"));
        }
    }
}

return new ChatCommand();
