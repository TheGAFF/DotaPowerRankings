namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceTeam
{
    public List<PlayerDataSourcePlayer> Players { get; set; } = default!;
    public string Name { get; set; } = null!;
}