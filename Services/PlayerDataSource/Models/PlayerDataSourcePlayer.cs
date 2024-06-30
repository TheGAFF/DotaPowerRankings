namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourcePlayer
{
    public long Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsCaptain { get; set; }
    public string CaptainName { get; set; } = default!;

    public int? Cost { get; set; }

    public decimal? EstimatedValue { get; set; }

    public string PlayerStatement { get; set; }
}