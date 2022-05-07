namespace RD2LPowerRankings.Database.Dota.Models;

public partial class PlayerMatchItemFirstPurchase
{
    public string Item { get; set; } = null!;
    public long MatchId { get; set; }
    public long PlayerId { get; set; }
    public long Time { get; set; }

    public virtual Match Match { get; set; } = null!;
}