using RD2LPowerRankings.Services.DotaRanking;

namespace RD2LPowerRankings.Modules.Dota;

public class PlayerReviewKeyValuePairs
{
    public static readonly List<KeyValuePair<int, string[]>> DescriptionThresholds =
        new()
        {
            new KeyValuePair<int, string[]>(10, new[]
            {
                $"god tier {PlayerReviewEnums.ReviewVariables.MetricER}",
                $"their {PlayerReviewEnums.ReviewVariables.MetricING} is practically perfect",
                $"their {PlayerReviewEnums.ReviewVariables.MetricING} is practically perfect",
            })
        };

    public static readonly List<KeyValuePair<decimal, string[]>> DescriptionLaneEfficiencyThresholds =
        new()
        {
            new KeyValuePair<decimal, string[]>(1.1M, new[]
            {
                "God tier laner" +
                "",
                "2"
            }),
            new KeyValuePair<decimal, string[]>(1.1M, new[]
            {
                "God tier laner" +
                "",
                "2"
            })
        };
}