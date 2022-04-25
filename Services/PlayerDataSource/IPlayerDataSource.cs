using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Services.PlayerDataSource;

public interface IPlayerDataSource
{
    public List<PlayerDataSourcePlayer> GetPlayers(string sheetId);

    public List<PlayerDataSourceTeam> GetTeams(string sheetId);
}