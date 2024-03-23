using Newtonsoft.Json;
using RD2LPowerRankings.Helpers;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedHero
{
    public DotaEnums.Hero HeroId { get; set; }
    public int MatchesPlayed { get; set; }
    public int TotalKills { get; set; }
    public int TotalDeaths { get; set; }
    public int TotalAssists { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal WinRate { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal KDA { get; set; }


    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal SafelanePercent { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal OfflanePercent { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal MidlanePercent { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal JunglePercent { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal RoamingPercent { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    [JsonIgnore]
    public decimal SoftSupportPercent { get; set; }

    [JsonIgnore] public decimal HardSupportPercent { get; set; }
    public decimal Score { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreMidlane { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreSafelane { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreOfflane { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreRoaming { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreJungle { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreSoftSupport { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ScoreHardSupport { get; set; }

    [JsonIgnore] public decimal Impact { get; set; }

    [JsonIgnore] public int? LeaderboardRank { get; set; }
    public int SkillAverageBadge { get; set; }
    [JsonIgnore] public decimal SoloRankedMatchMakingPercent { get; set; }
    [JsonIgnore] public decimal PartyRankedMatchMakingPercent { get; set; }
    [JsonIgnore] public decimal SoloNormalMatchMakingPercent { get; set; }
    [JsonIgnore] public decimal PartyNormalMatchMakingPercent { get; set; }
    [JsonIgnore] public decimal BattleCupMatchMakingPercent { get; set; }
    [JsonIgnore] public decimal LeagueMatchMakingPercent { get; set; }
}