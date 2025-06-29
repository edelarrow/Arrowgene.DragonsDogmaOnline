#load "libs.csx"

public class Mixin : IRentalCostMixin
{
    /// <summary>
    /// The base cost of a rental pawn scales linearly with their job level and craft rank.
    /// The cost for hiring higher level pawns (both combat and crafting) scales exponentially with the distance between that pawn and either your highest level job or your best crafting pawn.
    /// Pawns that are from clan-mates are discounted by 50%.
    /// </summary>

    public static readonly double JOB_PENALTY_MAGNITUDE = 100; // Set to 0.5 to disable this scaling.
    public static readonly double CRAFT_PENALTY_MAGNITUDE = 100; // Set to 0.5 to disable this scaling.
    public static readonly double JOB_LEVEL_PENALTY_RATE = Math.Pow(JOB_PENALTY_MAGNITUDE + 0.5, 1.0 / 99.0); // Approximately 100x cost at 99 level difference.
    public static readonly double CRAFT_LEVEL_PENALTY_RATE = Math.Pow(CRAFT_PENALTY_MAGNITUDE + 0.5, 1.0 / 75.0); // Approximately 100x cost at 75 craft rank difference.

    public static readonly double CLAN_DISCOUNT_FACTOR = 0.5;

    public override uint GetRentalCost(GameClient client, CDataRegisterdPawnList pawnListEntry, bool isClan)
    {
        uint maxLevel = client.Character.CharacterJobDataList.Select(x => x.Lv).Max();
        uint maxCraft = client.Character.Pawns.Select(x => x.CraftData.CraftRank).Max();

        uint deltaLevel = pawnListEntry.PawnListData.Level > maxLevel ? pawnListEntry.PawnListData.Level - maxLevel : 0;
        uint deltaCraft = pawnListEntry.PawnListData.CraftRank > maxCraft ? pawnListEntry.PawnListData.CraftRank - maxCraft : 0;

        // These penalties are 0.5 for pawns of equal or lower level; the total penalty is the sum of the two, so the
        double levelPenalty = Math.Pow(JOB_LEVEL_PENALTY_RATE, deltaLevel) - 0.5; 
        double craftPenalty = Math.Pow(CRAFT_LEVEL_PENALTY_RATE, deltaCraft) - 0.5;

        uint baseLevelCost = pawnListEntry.PawnListData.Level * 10;
        uint baseCraftCost = pawnListEntry.PawnListData.CraftRank * 15;

        double discountFactor = isClan ? CLAN_DISCOUNT_FACTOR : 1.0;

        uint adjustedCost = (uint)((baseLevelCost + baseCraftCost) * (levelPenalty + craftPenalty) * discountFactor);

        return adjustedCost;
    }
}

return new Mixin();
