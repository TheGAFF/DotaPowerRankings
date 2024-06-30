using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Database.Dota.Models;

public class PlayerMatchKill
{
    public long MatchId { get; set; }
    public long PlayerId { get; set; }
    public long? TargetId { get; set; }
    public DotaEnums.Hero PlayerHeroId { get; set; }
    public DotaEnums.Hero TargetHeroId { get; set; }
    public long? Time { get; set; }
    public long? ItemId { get; set; }
    public long? AbilityId { get; set; }

    public virtual Match Match { get; set; } = null!;
}