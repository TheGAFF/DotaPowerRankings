namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourcePlayer
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsCaptain { get; set; }
}