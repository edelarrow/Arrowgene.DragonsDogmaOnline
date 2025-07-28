public class ChatCommand : IChatCommand
{
    public override AccountStateType AccountState => AccountStateType.Admin;
    public override string CommandName => "playpoint";
    public override string HelpText => "usage: `/playpoint [jobid] [amount]` - Gain play points.";

    public override void Execute(DdonGameServer server, string[] command, GameClient client, ChatMessage message, List<ChatResponse> responses)
    {
        if (command.Length == 0)
        {
            responses.Add(ChatResponse.CommandError(client, "No arguments provided."));
            return;
        }

        JobId? targetJob = null;

        // Try by id
        if (Byte.TryParse(command[0], out byte parsedJobId))
        {
            targetJob = (JobId)parsedJobId;
        }
        else
        {
            string commandJobName = command[0].ToUpper();
            foreach (string jobName in Enum.GetNames(typeof(JobId)))
            {
                if (jobName.ToUpper() == commandJobName)
                {
                    targetJob = (JobId)Enum.Parse(typeof(JobId), jobName);
                    break;
                }
            }
        }

        if (targetJob == null)
        {
            responses.Add(ChatResponse.CommandError(client, "invalid job, try one of the following (job id or name):"));
            responses.Add(ChatResponse.CommandError(client, "1-Fighter"));
            responses.Add(ChatResponse.CommandError(client, "2-Seeker"));
            responses.Add(ChatResponse.CommandError(client, "3-Hunter"));
            responses.Add(ChatResponse.CommandError(client, "4-Priest"));
            responses.Add(ChatResponse.CommandError(client, "5-ShieldSage"));
            responses.Add(ChatResponse.CommandError(client, "6-Sorcerer"));
            responses.Add(ChatResponse.CommandError(client, "7-Warrior"));
            responses.Add(ChatResponse.CommandError(client, "8-ElementArcher"));
            responses.Add(ChatResponse.CommandError(client, "9-Alchemist"));
            responses.Add(ChatResponse.CommandError(client, "10-SpiritLancer"));
            responses.Add(ChatResponse.CommandError(client, "11-HighScepter"));
            return;
        }

        uint amount = 0;
        if (command.Length >= 2)
        {
            if (UInt32.TryParse(command[1], out uint parsedAmount))
            {
                amount = parsedAmount;
            }
            else
            {
                responses.Add(ChatResponse.CommandError(client, $"Invalid level \"{command[1]}\". It must be a number."));
                return;
            }
        }

        server.PPManager.AddPlayPointNtc(client, (amount, 0), targetJob);
    }
}

return new ChatCommand();
