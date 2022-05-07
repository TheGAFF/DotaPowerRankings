using RD2LPowerRankings.Services.DotaDataSource;

namespace RD2LPowerRankings.Database.Dota.Models;

public partial class UnParsedMatch
{
    public long MatchId { get; set; }
    public DotaDataEnums.MatchParseFailureReason FailureReason { get; set; }
    public bool ParseRequestSent { get; set; }
}