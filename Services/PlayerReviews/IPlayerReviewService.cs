using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaRanking;

public interface IPlayerReviewService
{
    public PowerRankedLeague GeneratePlayerReviews(PowerRankedLeague league);
}