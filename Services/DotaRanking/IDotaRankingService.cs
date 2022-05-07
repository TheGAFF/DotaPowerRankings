using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Modules.Dota;

public interface IDotaRankingService
{
    public PowerRankedLeague GeneratePostSeasonLeaguePowerRankings(PlayerDataSourceLeague league);

    public PowerRankedLeague GeneratePreSeasonLeaguePowerRankings(PlayerDataSourceLeague league);
}