using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Database.Dota.Models;

public class Match
{
    public Match()
    {
        PlayerMatchAbilities = new HashSet<PlayerMatchAbility>();
        PlayerMatchItemUses = new HashSet<PlayerMatchItemUse>();
        PlayerMatches = new HashSet<PlayerMatch>();
    }

    public long MatchId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool Loaded { get; set; }
    public int? Duration { get; set; }
    public DotaEnums.GameMode GameMode { get; set; }
    public int? LeagueId { get; set; }

    public int RadiantKills { get; set; }

    public int DireKills { get; set; }
    public DotaEnums.LobbyType? LobbyType { get; set; }

    public int? Rank { get; set; }
    public DotaEnums.Region? Region { get; set; }

    public int? StartTime { get; set; }


    public virtual ICollection<PlayerMatchAbility> PlayerMatchAbilities { get; set; }
    public virtual ICollection<PlayerMatchItemUse> PlayerMatchItemUses { get; set; }
    public virtual ICollection<PlayerMatch> PlayerMatches { get; set; }
}