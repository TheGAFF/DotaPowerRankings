namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedDivision
{
    public string Name { get; set; }
    public List<PowerRankedTeam> Teams { get; set; } = new();

    public List<PostSeasonAward> PostSeasonAwards { get; set; } = new();
}