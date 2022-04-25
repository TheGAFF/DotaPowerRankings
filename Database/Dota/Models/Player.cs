using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RD2LPowerRankings.Database.Dota.Models;

[Index(nameof(MmrEstimate))]
[Index(nameof(RankTier))]
public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long PlayerId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? PersonaName { get; set; }

    public string? DraftName { get; set; }

    public bool HasDotaPlus { get; set; }
    public int? Cheese { get; set; }

    public string? Steamid { get; set; }
    public string? Avatar { get; set; }

    public string? Profileurl { get; set; }

    public string? LastLogin { get; set; }
    public string? Loccountrycode { get; set; }

    public bool? IsContributor { get; set; }

    public int? LeaderboardRank { get; set; }
    public int? MmrEstimate { get; set; }
    public int? RankTier { get; set; }
}