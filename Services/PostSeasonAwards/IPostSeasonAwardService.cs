using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaRanking;

public interface IPostSeasonAwardService
{
    public PowerRankedLeague GeneratePostSeasonAwards(PowerRankedLeague league);
}