using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Services.PostSeasonAwards;

public interface IPostSeasonAwardService
{
    public PowerRankedLeague GeneratePostSeasonAwards(PowerRankedLeague league);

    public PowerRankedPlayer CalculatePostSeasonPlayerScore(PowerRankedPlayer powerRankedPlayer,
        PowerRankedLeague league, List<PlayerMatch> playerMatches);


    public PowerRankedPlayer CalculatePostSeasonHeroScore(PowerRankedPlayer powerRankedPlayer,
        PowerRankedLeague league, IGrouping<DotaEnums.Hero, PlayerMatch> heroMatches);
}