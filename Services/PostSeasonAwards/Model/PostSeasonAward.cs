using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PostSeasonAward
{
    public string Name { get; set; } = null!;
    public string Awardee { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string RunnerUp { get; set; } = null!;
    public string RunnerUpValue { get; set; } = null!;

    public DotaEnums.PostSeasonAwardCategory Category { get; set; }
}