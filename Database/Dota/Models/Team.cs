namespace RD2LPowerRankings.Database.Dota.Models;

public partial class Team
{
    public long TeamCaptainId { get; set; }
    public string SeasonName { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? TeamName { get; set; }
    public string? Description { get; set; }
}