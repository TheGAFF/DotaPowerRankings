﻿using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PostSeasonPlayerScore
{
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int TotalGames { get; set; }
    public int TotalNeutralKills { get; set; }
    public int TotalObsPlaced { get; set; }
    public int TotalSentriesPlaced { get; set; }
    public int TotalPings { get; set; }
    public int TotalTimeDead { get; set; }
    public int TotalRoshansKilled { get; set; }
    public int TotalRunesPickedUp { get; set; }
    public long TotalTowerDamage { get; set; }
    public long TotalGold { get; set; }
    public decimal TotalStunSeconds { get; set; }
    public int TotalAncientsKilled { get; set; }
    public int TotalCampsStacked { get; set; }
    public int TotalCourierKills { get; set; }
    public int TotalDenies { get; set; }
    public int TotalBuyBacks { get; set; }
    public int TotalFirstBloods { get; set; }
    public int TotalLastHits { get; set; }
    public long TotalHealing { get; set; }
    public int TotalAssists { get; set; }
    
    public decimal AverageTeamFightParticipation { get; set; }
    public decimal AverageActionsPerMin { get; set; }
    public decimal AverageLaneEffiencyPct { get; set; }
    
    public int ItemTotalDivineRapiers { get; set; }
    
    public int ItemTotalGemOfTrueSight { get; set; }
    
    public int ItemTotalBrownBoots { get; set; }
    
    public int TotalHeroesPlayed { get; set; }
    
    public long LongestWonGameLength { get; set; }
    public long ShortestWonGameLength { get; set; }
    
    public decimal HighestKDA { get; set; }
    public DotaEnums.Hero HighestKDAHero { get; set; }
    
    public int MostGamesOnSingleHero { get; set; }
    public DotaEnums.Hero MostGamesOnSingleHeroId { get; set; }
    
    
    
    
    


}