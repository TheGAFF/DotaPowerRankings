using System;
using System.Collections.Generic;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings;

public class PlayerMatch
{
    public long MatchId { get; set; }
    public long PlayerId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Abandons { get; set; }
    public long ActionsPerMinute { get; set; }
    public int Assists { get; set; }
    public int? Backpack0 { get; set; }
    public int? Backpack1 { get; set; }
    public int? Backpack2 { get; set; }
    public int? Backpack3 { get; set; }
    public int CampsStacked { get; set; }
    public int CourierKills { get; set; }
    public int Deaths { get; set; }
    public int Denies { get; set; }
    public long Duration { get; set; }
    public bool FirstbloodClaimed { get; set; }
    public int Gold { get; set; }
    public int GoldPerMin { get; set; }
    public int GoldSpent { get; set; }
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

    public bool IsRadiant { get; set; }
    public bool IsRoaming { get; set; }
    public decimal Kda { get; set; }
    public int Kills { get; set; }
    public DotaEnums.Lane Lane { get; set; }
    public int LaneEfficiencyPct { get; set; }
    public DotaEnums.TeamRole LaneRole { get; set; }
    public int LastHits { get; set; }
    public string LeaverStatus { get; set; }
    public int LeagueId { get; set; }
    public int Level { get; set; }
    public int LifeStateDead { get; set; }
    public DotaEnums.LobbyType? LobbyType { get; set; }
    public bool Lose { get; set; }
    public long NetWorth { get; set; }
    public int ObsPlaced { get; set; }
    public int Pings { get; set; }
    public int? PartyId { get; set; }
    public bool Randomed { get; set; }
    public int? MatchRank { get; set; }
    public DotaEnums.Region? Region { get; set; }
    public int RunePickups { get; set; }

    public int? Scans { get; set; }
    public int SenPlaced { get; set; }
    public int? Skill { get; set; }
    public long StartTime { get; set; }
    public long StunDuration { get; set; }
    public decimal TeamfightParticipation { get; set; }
    public long TotalGold { get; set; }
    public long TowerDamage { get; set; }
    public bool Win { get; set; }
    public int XpPerMin { get; set; }
    public int Imp { get; set; }
    public bool IntentionalFeeding { get; set; }
    public string Award { get; set; }
    public long GoldFed { get; set; }
    public long GoldLost { get; set; }
    public long SlowDuration { get; set; }
    public int SmokeKillCount { get; set; }
    public int SoloKillCount { get; set; }
    public int InvisibleKillCount { get; set; }
    public int TpRecentlyKillCount { get; set; }
    public int GankKillCount { get; set; }
    public int DeathTpAttemptCount { get; set; }
    public int PauseCount { get; set; }
    public virtual Match Match { get; set; } = new();
}