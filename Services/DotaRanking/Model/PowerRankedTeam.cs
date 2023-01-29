using Newtonsoft.Json;
using RD2LPowerRankings.Helpers;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedTeam
{
    public List<PowerRankedPlayer> Players { get; set; } = new();

    [JsonConverter(typeof(DecimalFormatConverter))]
    public decimal TotalScore { get; set; }

    public int BadgeAverage { get; set; }

    public decimal EstimatedMMRAverage { get; set; }

    public int Rank { get; set; }
    public int DivisionRank { get; set; }
    public string DivisionName { get; set; } = default!;

    public string Name { get; set; } = null!;

    public TeamReview TeamReview { get; set; } = new();

    public List<PowerRankedAward> Awards { get; set; } = new();
}