using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Database.Dota.Models;

[Index(nameof(MatchId))]
[Index(nameof(PlayerId))]
[Index(nameof(HeroId))]
[Index(nameof(StartTime))]
[Index(nameof(PlayerSlot))]
public class PlayerMatch
{
    public int PlayerSlot { get; set; }

    public long MatchId { get; set; }
    [ForeignKey(nameof(MatchId))] public virtual Match Match { get; set; } = default!;
    public long PlayerId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Abandons { get; set; }
    public int[] AbilityUpgradesArr { get; set; } = default!;
    public long ActionsPerMinute { get; set; }
    public int AncientKills { get; set; }
    public int Assists { get; set; }
    public int? Backpack0 { get; set; }
    public int? Backpack1 { get; set; }
    public int? Backpack2 { get; set; }
    public int? Backpack3 { get; set; }
    public int? BuyBackCount { get; set; }
    public int CampsStacked { get; set; }
    public int? Cluster { get; set; }
    public int CourierKills { get; set; }
    public int DisconnectCount { get; set; }
    public int DisconnectTotalTime { get; set; }
    public int CreepsStacked { get; set; }
    public int Deaths { get; set; }
    public int Denies { get; set; }
    public int[] DnT { get; set; } = default!;
    public long Duration { get; set; }
    public bool FirstbloodClaimed { get; set; }
    public int Gold { get; set; }
    public int GoldPerMin { get; set; }
    public int GoldSpent { get; set; }
    public int[] GoldT { get; set; } = default!;
    public long HeroDamage { get; set; }
    public long HeroHealing { get; set; }
    public DotaEnums.Hero HeroId { get; set; }
    public DotaEnums.Item? Item0 { get; set; }
    public DotaEnums.Item? Item1 { get; set; }
    public DotaEnums.Item? Item2 { get; set; }
    public DotaEnums.Item? Item3 { get; set; }
    public DotaEnums.Item? Item4 { get; set; }
    public DotaEnums.Item? Item5 { get; set; }
    public DotaEnums.Item? ItemNeutral { get; set; }
    public bool IsRoaming { get; set; }
    public decimal Kda { get; set; }
    public int Kills { get; set; }
    public decimal KillsPerMin { get; set; }
    public DotaEnums.Lane Lane { get; set; }
    public int LaneEfficiencyPct { get; set; }
    public int LaneKills { get; set; }
    public int? LaneRole { get; set; }
    public int LastHits { get; set; }
    public int LeaverStatus { get; set; }

    public int LeagueId { get; set; }
    public int Level { get; set; }
    public int[] LhT { get; set; } = default!;
    public int LifeStateDead { get; set; }
    public DotaEnums.LobbyType? LobbyType { get; set; }
    public bool Lose { get; set; }
    public int MultiKills2 { get; set; }
    public int MultiKills3 { get; set; }
    public int MultiKills4 { get; set; }
    public int MultiKills5 { get; set; }

    public long NetWorth { get; set; }
    public int NeutralKills { get; set; }
    public int ObsPlaced { get; set; }
    public int ObserverKills { get; set; }
    public int ObserverUses { get; set; }
    public int Pings { get; set; }
    public int? PartyId { get; set; }
    public int? PartySize { get; set; }

    public bool PredVict { get; set; }
    public bool Randomed { get; set; }
    public int? RankTier { get; set; }
    public DotaEnums.Region? Region { get; set; }
    public bool Repicked { get; set; }
    public int RoshanKills { get; set; }
    public int RoshansKilled { get; set; }
    public int RunePickups { get; set; }
    public int SenPlaced { get; set; }

    public int SentryKills { get; set; }
    public int SentryUses { get; set; }

    public int? Skill { get; set; }
    public long StartTime { get; set; }
    public decimal Stuns { get; set; }
    public decimal TeamfightParticipation { get; set; }
    public int[] Times { get; set; } = default!;
    public long TotalGold { get; set; }
    public long TotalXp { get; set; }
    public long TowerDamage { get; set; }
    public int TowerKills { get; set; }
    public int TowersKilled { get; set; }
    public bool Win { get; set; }
    public int XpPerMin { get; set; }
    public int[] XpT { get; set; } = default!;
    public virtual IList<PlayerMatchAbility>? AbilityUses { get; set; }
    public virtual IList<PlayerMatchAction>? Actions { get; set; }
    public virtual IList<PlayerMatchItemFirstPurchase>? FirstPurchases { get; set; }
    public virtual IList<PlayerMatchItemUse>? ItemUses { get; set; }
}