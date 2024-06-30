using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Services.DotaRanking;

public interface IDotaRankingService
{
    public Task<PowerRankedLeague> GeneratePostSeasonLeaguePowerRankings(PlayerDataSourceLeague league);

    public Task<PowerRankedLeague> GeneratePreSeasonLeaguePowerRankings(PlayerDataSourceLeague league);
}