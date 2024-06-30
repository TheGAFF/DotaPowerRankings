namespace RD2LPowerRankings.Database.Dota.Models;

public class PlayerDescription
{
    public long PlayerId { get; set; }

    public string SeasonName { get; set; }

    public string Description { get; set; }

    public string Prompt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Player Player { get; set; } = null!;
}