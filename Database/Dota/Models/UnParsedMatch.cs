using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RD2LPowerRankings.Database.Dota.Models;

public class UnParsedMatch

{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public long MatchId { get; set; }
}