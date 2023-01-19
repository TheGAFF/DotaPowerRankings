using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Database.Dota.Models;

public partial class PlayerMatchItemUse
{
    public long ItemId { get; set; }
    public long MatchId { get; set; }
    public long PlayerId { get; set; }
    public long Uses { get; set; }

    public virtual Match Match { get; set; } = null!;
}