using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RD2LPowerRankings.Database.Dota.Models;

[Index(nameof(Word))]
[Index(nameof(PlayerId))]
public class PlayerWord
{
    public string Word { get; init; } = null!;

    public int Count { get; set; }

    public long PlayerId { get; set; }
    [ForeignKey(nameof(PlayerId))] public virtual Player? Player { get; set; }

    public DateTime UpdatedAt { get; set; }
}