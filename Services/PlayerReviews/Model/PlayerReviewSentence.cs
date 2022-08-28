using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PlayerReviewSentence<T>
{
    public T Threshold { get; set; }

    public int Rating { get; set; }
}