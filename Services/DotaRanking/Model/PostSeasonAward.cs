using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PostSeasonAward
{
    public string Name { get; set; }
    public string Awardee { get; set; }
    public string Value { get; set; }
    public string RunnerUp { get; set; }
    public string RunnerUpValue { get; set; }

    public DotaEnums.PostSeasonAwardCategory Category { get; set; }
}