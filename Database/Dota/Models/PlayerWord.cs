namespace RD2LPowerRankings.Database.Dota.Models;

public partial class PlayerWord
{
    public string Word { get; set; } = null!;
    public long PlayerId { get; set; }
    public int Count { get; set; }
    public DateTime UpdatedAt { get; set; }

    public virtual Player Player { get; set; } = null!;
}