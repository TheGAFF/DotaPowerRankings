using System;
using System.Collections.Generic;
using RD2LPowerRankings.Database.Dota.Models;

namespace RD2LPowerRankings;

public partial class PlayerMatchAction
{
    public string Action { get; set; } = null!;
    public long MatchId { get; set; }
    public long PlayerId { get; set; }
    public long Count { get; set; }

    public virtual Match Match { get; set; } = null!;
}