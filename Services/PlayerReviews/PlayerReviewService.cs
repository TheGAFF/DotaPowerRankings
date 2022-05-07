using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaRanking;

public class PlayerReviewService : IPlayerReviewService

{
    private readonly ILogger<PlayerReviewService> _logger;

    public PlayerReviewService(ILogger<PlayerReviewService> logger)
    {
        _logger = logger;
    }

    public PowerRankedLeague GeneratePlayerReviews(PowerRankedLeague league)
    {
        var players = league.Divisions.SelectMany(x => x.Teams).SelectMany(x => x.Players).ToList();
        foreach (var player in players)
        {
            player.PlayerReview = GivePlayerReview(player, players);
        }


        return league;
    }


    private PlayerReview GivePlayerReview(PowerRankedPlayer player, IList<PowerRankedPlayer> players)
    {
        return player.PlayerReview;
    }
}