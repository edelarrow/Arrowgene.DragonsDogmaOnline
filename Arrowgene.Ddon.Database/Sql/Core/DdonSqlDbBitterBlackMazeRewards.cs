using Arrowgene.Ddon.Shared.Model.BattleContent;
using System.Collections.Generic;
using System.Data.Common;

namespace Arrowgene.Ddon.Database.Sql.Core;

public partial class DdonSqlDb : SqlDb
{
    protected static readonly string[] BitterBlackMazeRewardsFields = new[]
    {
        "character_id", "stage_id", "gold_marks", "silver_marks", "red_marks"
    };

    private readonly string SqlDeleteBBMRewards = "DELETE FROM \"ddon_bbm_rewards\" WHERE \"character_id\"=@character_id;";

    private readonly string SqlInsertBBMRewards =
        $"INSERT INTO \"ddon_bbm_rewards\" ({BuildQueryField(BitterBlackMazeRewardsFields)}) VALUES ({BuildQueryInsert(BitterBlackMazeRewardsFields)});";

    private readonly string SqlSelectBBMRewards = $"SELECT {BuildQueryField(BitterBlackMazeRewardsFields)} FROM \"ddon_bbm_rewards\" WHERE \"character_id\"=@character_id;";

    private readonly string SqlUpdateBBMRewards = $"UPDATE \"ddon_bbm_rewards\" SET {BuildQueryUpdate(BitterBlackMazeRewardsFields)} WHERE \"character_id\"=@character_id AND \"stage_id\"=@stage_id;";

    private readonly string SqlInsertBBMResetTicket = "INSERT INTO \"ddon_bbm_reset_ticket\" (character_id) VALUES (@character_id) ON CONFLICT DO NOTHING;";
    private readonly string SqlResetBBMResetTicket = "DELETE FROM \"ddon_bbm_reset_ticket\";";

    public override bool InsertBBMRewards(uint characterId, uint goldMarks, uint silverMarks, uint redMarks, uint stageId, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection, SqlInsertBBMRewards, command =>
            {
                AddParameter(command, "character_id", characterId);
                AddParameter(command, "stage_id", stageId);
                AddParameter(command, "gold_marks", goldMarks);
                AddParameter(command, "silver_marks", silverMarks);
                AddParameter(command, "red_marks", redMarks);
            }) == 1;
        });
    }

    public override bool UpdateBBMRewards(uint characterId, BitterblackMazeMarkRewards rewards, DbConnection? connectionIn = null)
    {
        return UpdateBBMRewards(characterId, rewards.GoldMarks, rewards.SilverMarks, rewards.RedMarks, rewards.StageId, connectionIn);
    }

    public bool UpdateBBMRewards(uint characterId, uint goldMarks, uint silverMarks, uint redMarks, uint stageId, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection, SqlUpdateBBMRewards, command =>
            {
                AddParameter(command, "character_id", characterId);
                AddParameter(command, "stage_id", stageId);
                AddParameter(command, "gold_marks", goldMarks);
                AddParameter(command, "silver_marks", silverMarks);
                AddParameter(command, "red_marks", redMarks);
            }) == 1;
        });
    }

    public override bool RemoveBBMRewards(uint characterId, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection, SqlDeleteBBMRewards, command => { AddParameter(command, "character_id", characterId); }) == 1;
        });
    }

    public override Dictionary<uint, BitterblackMazeMarkRewards> SelectBBMRewards(uint characterId, DbConnection? connectionIn = null)
    {
        Dictionary<uint, BitterblackMazeMarkRewards> result = [];

        ExecuteQuerySafe(connectionIn, connection =>
        {
            ExecuteReader(connection, SqlSelectBBMRewards, command => { AddParameter(command, "character_id", characterId); }, reader =>
            {
                while (reader.Read())
                {
                    var stageId = GetUInt32(reader, "stage_id");

                    result.Add(stageId, new()
                    {
                        GoldMarks = GetUInt32(reader, "gold_marks"),
                        SilverMarks = GetUInt32(reader, "silver_marks"),
                        RedMarks = GetUInt32(reader, "red_marks"),
                        StageId = stageId,
                    });
                }
            });
        });

        return result;
    }

    public override bool InsertBBMResetTicketStatus(uint characterId, DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection,
                SqlInsertBBMResetTicket,
                command => { AddParameter(command, "@character_id", characterId); }
            ) == 1;
        });
    }

    public override bool ResetBBMResetTicketStatus(DbConnection? connectionIn = null)
    {
        return ExecuteQuerySafe(connectionIn, connection =>
        {
            return ExecuteNonQuery(connection,
                SqlResetBBMResetTicket,
                command => { }) > 0;
        });
    }
}
