using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RD2LPowerRankings.Database.Dota.Models;

[Index(nameof(MatchId))]
[Index(nameof(PlayerId))]
public class PlayerMatchAbility
{
    public string Ability { get; set; } = default!;
    public long Count { get; set; }

    public long MatchId { get; set; }

    [ForeignKey(nameof(MatchId))] public virtual Match? Match { get; set; }
    public long PlayerId { get; set; }
}