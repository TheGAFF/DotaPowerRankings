using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RD2LPowerRankings.Services.DotaDataSource;

namespace RD2LPowerRankings.Database.Dota.Models;

public class UnParsedMatch

{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public long MatchId { get; set; }
    
    public DotaDataEnums.MatchParseFailureReason FailureReason { get; set; }
    
    
    public bool ParseRequestSent { get; set; }
}