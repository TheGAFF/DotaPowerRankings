namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedDivision
{
    public string Name { get; set; } = null!;
    public List<PowerRankedTeam> Teams { get; set; } = new();

    public List<PowerRankedPlayer> Players { get; set; } = new();

    public List<PostSeasonAward> PostSeasonAwards { get; set; } = new();
}