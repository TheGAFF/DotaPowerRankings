namespace RD2LPowerRankings.Database.Dota.Models;

public partial class Player
{
    public Player()
    {
        PlayerWords = new HashSet<PlayerWord>();
    }

    public long PlayerId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? PersonaName { get; set; }
    public string? DraftName { get; set; }
    public int? Cheese { get; set; }
    public bool HasDotaPlus { get; set; }
    public string? Steamid { get; set; }
    public string? Avatar { get; set; }
    public string? Profileurl { get; set; }
    public string? LastLogin { get; set; }
    public string? Loccountrycode { get; set; }
    public bool? IsContributor { get; set; }
    public int? LeaderboardRank { get; set; }
    public int? RankTier { get; set; }
    public int? MmrEstimate { get; set; }
    public int? SmurfFlag { get; set; }
    public int? SoloRank { get; set; }
    public int? PartyRank { get; set; }
    public int? LastMatchParseDate { get; set; }
    public int? BehaviorScore { get; set; }

    public string? Description { get; set; }
    public virtual ICollection<PlayerWord> PlayerWords { get; set; }
}