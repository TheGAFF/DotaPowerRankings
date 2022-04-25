using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Database.Dota.Models;

[Index(nameof(GameMode))]
[Index(nameof(Leagueid))]
[Index(nameof(LobbyType))]
[Index(nameof(Skill))]
public class Match

{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public long MatchId { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public bool Loaded { get; set; }

    public int? BarracksStatusDire { get; set; }
    public int? BarracksStatusRadiant { get; set; }
    public int? Cluster { get; set; }
    public int? DireScore { get; set; }
    public int? DireTeamId { get; set; }
    public int? Duration { get; set; }
    public int? Engine { get; set; }
    public int? FirstBloodTime { get; set; }
    public DotaEnums.GameMode? GameMode { get; set; }
    public int? HumanPlayers { get; set; }
    public int? Leagueid { get; set; }
    public DotaEnums.LobbyType? LobbyType { get; set; }
    public long? Loss { get; set; }
    public long? MatchSeqNum { get; set; }
    public int? NegativeVotes { get; set; }
    public int? Patch { get; set; }
    public int? PositiveVotes { get; set; }
    public long[] RadiantGoldAdv { get; set; } = default!;
    public int? RadiantScore { get; set; }
    public int? RadiantTeamId { get; set; }
    public bool? RadiantWin { get; set; }
    public long[] RadiantXpAdv { get; set; } = default!;
    public DotaEnums.Region? Region { get; set; }
    public long? ReplaySalt { get; set; }
    public string? ReplayUrl { get; set; }
    public int? SeriesId { get; set; }
    public int? SeriesType { get; set; }
    public int? Skill { get; set; }
    public int? StartTime { get; set; }
    public int? Throw { get; set; }
    public int? TowerStatusRadiant { get; set; }
    public int? TowerStatusDire { get; set; }
    public int? Version { get; set; }

    public virtual IList<PlayerMatch>? Players { get; set; }
}