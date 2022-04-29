namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedLeague
{
    public string? Name { get; set; }
    
    public int? LeagueId { get; set; }
    public List<PowerRankedDivision> Divisions { get; set; } = new();
}