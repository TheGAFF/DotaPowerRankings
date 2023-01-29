using Newtonsoft.Json;
using RD2LPowerRankings.Helpers;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedPlayer
{
    public long PlayerId { get; set; }

    [JsonIgnore] public DateTime? CreatedAt { get; set; }

    public string? DraftName { get; set; }

    public string? PersonaName { get; set; }

    public string? Avatar { get; set; }

    [JsonIgnore] public int? SmurfFlag { get; set; }

    public string? Loccountrycode { get; set; }
    public int? MmrEstimate { get; set; }

    public int? RankTier { get; set; }

    public int? LeaderboardRank { get; set; }

    public bool IsCaptain { get; set; }

    public int OverallRank { get; set; }

    public int MidlaneRank { get; set; }

    public int SafelaneRank { get; set; }

    public int RoamingRank { get; set; }

    public int SoftSupportRank { get; set; }

    public int HardSupportRank { get; set; }
    public int OfflaneRank { get; set; }

    public int JungleRank { get; set; }

    public int ToxicityRank { get; set; }

    public int OverallDivisionRank { get; set; }

    public int MidlaneDivisionRank { get; set; }

    public int SafelaneDivisionRank { get; set; }

    public int RoamingDivisionRank { get; set; }

    public int SoftSupportDivisionRank { get; set; }

    public int HardSupportDivisionRank { get; set; }

    public int OfflaneDivisionRank { get; set; }

    public int JungleDivisionRank { get; set; }

    public int ToxicityDivisionRank { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal OverallScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal MidlaneScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal SafelaneScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal RoamingScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal SoftSupportScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal HardSupportScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal OfflaneScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal JungleScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal ToxicityScore { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal HeroVersatility { get; set; }

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal LaneVersatility { get; set; }

    [JsonIgnore] public decimal AverageWordToxicity { get; set; }
    [JsonIgnore] public decimal FirstBloodAverage { get; set; }

    [JsonIgnore] public decimal CourierKillAverage { get; set; }

    [JsonIgnore] public decimal CampsStackedAverage { get; set; }

    [JsonIgnore] public decimal DeniesAverage { get; set; }

    public List<DotaEnums.Hero> RespectBans { get; set; } = new();

    public List<PowerRankedHero> Heroes { get; set; } = new();

    public DotaEnums.TeamRole? TeamRole { get; set; }

    public string TeamName { get; set; } = null!;

    public List<PowerRankedAward> Awards { get; set; } = new();

    public string DivisionName { get; set; } = null!;

    [JsonIgnore] public decimal MatchRussiaPercent { get; set; }

    [JsonIgnore] public decimal MatchUSEastPercent { get; set; }

    [JsonIgnore] public decimal MatchUSWestPercent { get; set; }

    [JsonIgnore] public decimal MatchEUEastPercent { get; set; }

    [JsonIgnore] public decimal MatchEUWestPercent { get; set; }

    [JsonIgnore] public decimal MatchPeruPercent { get; set; }

    [JsonIgnore] public decimal AverageAbandons { get; set; }

    [JsonIgnore] public decimal AverageExcessPings { get; set; }

    [JsonIgnore] public decimal AverageExcessPingAbandons { get; set; }

    [JsonIgnore] public decimal AverageAPM { get; set; }

    [JsonIgnore] public decimal AverageTowerDamage { get; set; }

    [JsonIgnore] public decimal AverageStuns { get; set; }

    [JsonIgnore] public decimal AverageTeamFightParticipation { get; set; }

    [JsonIgnore] public decimal AverageLaneEfficiency { get; set; }

    [JsonIgnore] public decimal AverageLaneEfficiencyMid { get; set; }

    [JsonIgnore] public decimal AverageLaneEfficiencyOff { get; set; }

    [JsonIgnore] public decimal AverageLaneEfficiencySafe { get; set; }

    [JsonIgnore] public decimal AverageSentriesPlaced { get; set; }

    [JsonIgnore] public decimal AverageArmletToggles { get; set; }

    [JsonIgnore] public decimal AverageBlinkPurchases { get; set; }

    [JsonIgnore] public decimal AverageRapierPurchases { get; set; }

    [JsonIgnore] public decimal AverageGankKills { get; set; }

    [JsonIgnore] public decimal AverageSmokeKills { get; set; }

    [JsonIgnore] public decimal AverageSoloKills { get; set; }

    [JsonIgnore] public decimal AverageIntentionalFeeding { get; set; }

    [JsonIgnore] public decimal AverageTpKills { get; set; }

    [JsonIgnore] public decimal AverageMVPs { get; set; }

    [JsonIgnore] public decimal AverageBestSupport { get; set; }
    [JsonIgnore] public decimal AverageBestCore { get; set; }

    [JsonIgnore] public decimal AverageBestPos1 { get; set; }

    [JsonIgnore] public decimal AverageBestPos2 { get; set; }

    [JsonIgnore] public decimal AverageBestPos3 { get; set; }

    [JsonIgnore] public decimal AverageBestPos4 { get; set; }

    [JsonIgnore] public decimal AverageBestPos5 { get; set; }
    [JsonIgnore] public decimal AveragePauses { get; set; }

    [JsonIgnore] public decimal AverageRandomHeroes { get; set; }

    [JsonIgnore] public decimal AverageScans { get; set; }

    [JsonIgnore] public decimal AverageInvisibleKills { get; set; }

    [JsonIgnore] public PostSeasonPlayerScore PostSeasonPlayerScore { get; set; } = new();
    public PlayerReview PlayerReview { get; set; } = new();
}