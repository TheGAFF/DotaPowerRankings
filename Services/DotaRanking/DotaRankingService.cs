using System.Collections.Concurrent;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Modules.Dota;
using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.DotaAwards;
using RD2LPowerRankings.Services.DotaDataSource;
using RD2LPowerRankings.Services.DotaRanking.Enums;
using RD2LPowerRankings.Services.PlayerDataSource;
using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Services.DotaRanking;

public class DotaRankingService : IDotaRankingService
{
    private readonly DotaDbContext _context;
    private readonly ILogger<DotaRankingService> _logger;
    private readonly IMapper _mapper;
    private readonly IPlayerDataSource _playerDataSource;
    private readonly IPostSeasonAwardService _postSeasonAwardService;
    private readonly IDotaAwardsService _dotaAwardsService;

    public DotaRankingService(ILogger<DotaRankingService> logger, DotaDbContext context, IMapper mapper,
        IPlayerDataSource playerDataSource, IPostSeasonAwardService postSeasonAwardService,
        IDotaAwardsService dotaAwardsService)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _playerDataSource = playerDataSource;
        _postSeasonAwardService = postSeasonAwardService;
        _dotaAwardsService = dotaAwardsService;
    }


    public PowerRankedLeague GeneratePreSeasonLeaguePowerRankings(PlayerDataSourceLeague league)
    {
        var powerRankedLeague = new PowerRankedLeague { Name = league.Name, LeagueId = league.LeagueId };

        foreach (var division in league.Divisions)
        {
            var powerRankedDivision = new PowerRankedDivision { Name = division.Name };

            var playerIds = _playerDataSource.GetPlayers(division.SheetId).Select(x => x.Id).ToArray();

            powerRankedDivision.Players = GeneratePlayersStats(playerIds, powerRankedLeague, powerRankedDivision);

            powerRankedDivision.Players = _dotaAwardsService.GiveDivisionPlayerAwards(powerRankedDivision.Players);

            powerRankedDivision.Players = RankPlayers(powerRankedDivision.Players, powerRankedDivision);

            powerRankedLeague.Divisions.Add(powerRankedDivision);
        }


        return powerRankedLeague;
    }


    public PowerRankedLeague GeneratePostSeasonLeaguePowerRankings(PlayerDataSourceLeague league)
    {
        var powerRankedLeague = new PowerRankedLeague { Name = league.Name, LeagueId = league.LeagueId };

        foreach (var division in league.Divisions)
        {
            var powerRankedDivision = new PowerRankedDivision { Name = division.Name };

            division.Teams = _playerDataSource.GetTeams(division.SheetId);

            var players = GeneratePlayersStats(division.Teams.SelectMany(x => x.Players).Select(x => x.Id).ToArray(),
                powerRankedLeague, powerRankedDivision);

            foreach (var team in division.Teams)
            {
                var powerRankedTeam = new PowerRankedTeam();
                powerRankedTeam.DivisionName = division.Name;
                powerRankedTeam.Players =
                    players.Where(x => team.Players.Select(y => y.Id).Contains(x.PlayerId)).ToList();

                powerRankedTeam = MapTeamSheetToTeam(powerRankedTeam, team);

                powerRankedTeam = GetTeamRoles(powerRankedTeam);

                powerRankedDivision.Teams.Add(powerRankedTeam);
            }

            powerRankedDivision.Players = RankPlayers(powerRankedLeague.Divisions.SelectMany(x => x.Players).ToList(),
                powerRankedDivision);

            powerRankedDivision.Players = _dotaAwardsService.GiveDivisionPlayerAwards(powerRankedDivision.Players);

            powerRankedDivision = _dotaAwardsService.GiveDivisionTeamAwards(powerRankedDivision);

            powerRankedLeague.Divisions.Add(powerRankedDivision);
        }


        powerRankedLeague = RankTeams(powerRankedLeague);

        powerRankedLeague = _postSeasonAwardService.GeneratePostSeasonAwards(powerRankedLeague);

        return powerRankedLeague;
    }

    private List<PowerRankedPlayer> GeneratePlayersStats(long[] playerIds, PowerRankedLeague league,
        PowerRankedDivision division)
    {
        var powerRankedPlayers = new ConcurrentQueue<PowerRankedPlayer>();

        var cutOffDate = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromDays(DotaDataConstants.CutOffDateDays))
            .ToUnixTimeSeconds();

        var validLeagueIds = DotaRankingKeyValuePairs.ValidLeagues.Select(x => x.Key).ToArray();

        var players = _context.Players.AsNoTracking().Where(x => playerIds.Any(y => y == x.PlayerId)).ToList();
        var playerMatches = _context.PlayerMatches.AsNoTracking()
            .Where(x => x.StartTime >= cutOffDate &&
                        (x.LobbyType != DotaEnums.LobbyType.Practice ||
                         validLeagueIds.Contains(x.LeagueId)))
            .ToList();
        var playerWords = _context.PlayerWords.AsNoTracking().ToList();

        Parallel.ForEach(playerIds, new ParallelOptions { MaxDegreeOfParallelism = 6 },
            playerId =>
            {
                var player = players.FirstOrDefault(x => x.PlayerId == playerId);
                if (player == null)
                {
                    return;
                }

                powerRankedPlayers.Enqueue(
                    GeneratePowerRankedPlayer(
                        player,
                        playerMatches.Where(x => x.PlayerId == playerId).ToList(),
                        playerWords.Where(x => x.PlayerId == playerId).ToList(),
                        league,
                        division
                    ));
            });

        return powerRankedPlayers.ToList();
    }

    private PowerRankedPlayer GeneratePowerRankedPlayer(Player player, List<PlayerMatch> playerMatches,
        List<PlayerWord> playerWords, PowerRankedLeague league, PowerRankedDivision division)
    {
        var powerRankedPlayer = _mapper.Map<PowerRankedPlayer>(player);
        powerRankedPlayer.DivisionName = division.Name;

        powerRankedPlayer.AverageWordToxicity = GetPlayerAverageWordToxicity(playerWords, playerMatches.Count);


        powerRankedPlayer.FirstBloodAverage =
            (decimal)playerMatches.Count(x => x.FirstbloodClaimed) / playerMatches.Count * 1M;

        powerRankedPlayer.CourierKillAverage =
            (decimal)playerMatches.Sum(x => x.CourierKills) / playerMatches.Count;

        powerRankedPlayer.CreepsStackedAverage =
            (decimal)playerMatches.Sum(x => x.CreepsStacked) / playerMatches.Count;

        powerRankedPlayer.DeniesAverage =
            (decimal)playerMatches.Sum(x => x.Denies) / playerMatches.Count;

        powerRankedPlayer.MatchEUEastPercent = (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.Austria) /
            playerMatches.Count * 1M;
        powerRankedPlayer.MatchEUWestPercent = (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.Europe) /
            playerMatches.Count * 1M;
        powerRankedPlayer.MatchUSEastPercent = (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.USEast) /
            playerMatches.Count * 1M;
        powerRankedPlayer.MatchUSWestPercent = (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.USWest) /
            playerMatches.Count * 1M;
        powerRankedPlayer.MatchRussiaPercent =
            (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.Stockholm) / playerMatches.Count * 1M;
        powerRankedPlayer.MatchPeruPercent = (decimal)playerMatches.Count(x => x.Region == DotaEnums.Region.Peru) /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageAbandons =
            (decimal)playerMatches.Sum(x => x.Abandons) / playerMatches.Count;

        powerRankedPlayer.AverageExcessPings = (decimal)playerMatches
            .Where(x => x.Pings > DotaRankingConstants.ExcessivePingThreshold)
            .Sum(x => x.Pings) / playerMatches.Count;

        powerRankedPlayer.AverageExcessPingAbandons =
            (decimal)playerMatches.Where(x => x.Pings > DotaRankingConstants.ExcessivePingThreshold && x.Abandons == 1)
                .Sum(x => x.Pings) / playerMatches.Count;

        powerRankedPlayer.AverageTeamFightParticipation = playerMatches.Average(x => x.TeamfightParticipation);
        powerRankedPlayer.AverageStuns = playerMatches.Average(x => x.Stuns);
        powerRankedPlayer.AverageAPM = (decimal)playerMatches.Average(x => x.ActionsPerMinute);
        powerRankedPlayer.AverageTowerDamage = (decimal)playerMatches.Average(x => x.TowerDamage);
        powerRankedPlayer.AverageDisconnects = (decimal)playerMatches.Average(x => x.DisconnectCount);

        if (league.LeagueId.HasValue && playerMatches.Count(x => x.LeagueId == league.LeagueId.Value) > 0)
        {
            powerRankedPlayer.PostSeasonPlayerScore.Losses =
                playerMatches.Count(x => x.LeagueId == league.LeagueId.Value && x.Lose);
            powerRankedPlayer.PostSeasonPlayerScore.Wins =
                playerMatches.Count(x => x.LeagueId == league.LeagueId.Value && x.Win);
            powerRankedPlayer.PostSeasonPlayerScore.TotalGames =
                playerMatches.Count(x => x.LeagueId == league.LeagueId.Value);
            powerRankedPlayer.PostSeasonPlayerScore.TotalAssists =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.Assists);
            powerRankedPlayer.PostSeasonPlayerScore.TotalDenies =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.Denies);
            powerRankedPlayer.PostSeasonPlayerScore.TotalGold =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.TotalGold);
            powerRankedPlayer.PostSeasonPlayerScore.TotalHealing =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.HeroHealing);
            powerRankedPlayer.PostSeasonPlayerScore.TotalPings =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.Pings);
            powerRankedPlayer.PostSeasonPlayerScore.TotalAncientsKilled =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.AncientKills);
            powerRankedPlayer.PostSeasonPlayerScore.TotalBuyBacks =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.BuyBackCount ?? 0);
            powerRankedPlayer.PostSeasonPlayerScore.TotalCampsStacked =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.CampsStacked);
            powerRankedPlayer.PostSeasonPlayerScore.TotalCourierKills =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.CourierKills);
            powerRankedPlayer.PostSeasonPlayerScore.TotalFirstBloods =
                playerMatches.Count(x => x.LeagueId == league.LeagueId.Value && x.FirstbloodClaimed);
            powerRankedPlayer.PostSeasonPlayerScore.TotalLastHits =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.LastHits);
            powerRankedPlayer.PostSeasonPlayerScore.TotalNeutralKills =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.NeutralKills);
            powerRankedPlayer.PostSeasonPlayerScore.TotalObsPlaced =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.ObsPlaced);
            powerRankedPlayer.PostSeasonPlayerScore.TotalRunesPickedUp =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.RunePickups);
            powerRankedPlayer.PostSeasonPlayerScore.TotalRoshansKilled =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.RoshanKills);
            powerRankedPlayer.PostSeasonPlayerScore.TotalSentriesPlaced =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.SenPlaced);
            powerRankedPlayer.PostSeasonPlayerScore.TotalStunSeconds =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.Stuns);
            powerRankedPlayer.PostSeasonPlayerScore.TotalTimeDead =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.LifeStateDead);
            powerRankedPlayer.PostSeasonPlayerScore.TotalTowerDamage =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.TowerDamage);
            powerRankedPlayer.PostSeasonPlayerScore.AverageActionsPerMin =
                (decimal)playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.ActionsPerMinute) /
                powerRankedPlayer.PostSeasonPlayerScore.TotalGames * 1M;
            powerRankedPlayer.PostSeasonPlayerScore.AverageTeamFightParticipation = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.TeamfightParticipation);
            powerRankedPlayer.PostSeasonPlayerScore.AverageLaneEffiencyPct =
                (decimal)playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.LaneEfficiencyPct) /
                powerRankedPlayer.PostSeasonPlayerScore.TotalGames * 1M;
            powerRankedPlayer.PostSeasonPlayerScore.TotalHeroesPlayed =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).GroupBy(x => x.HeroId).Count();
            powerRankedPlayer.PostSeasonPlayerScore.LongestWonGameLength =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value && x.Win).Max(x => x.Duration);
            powerRankedPlayer.PostSeasonPlayerScore.ShortestWonGameLength = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value && x.Win && x.Duration > 0).Min(x => x.Duration);
            powerRankedPlayer.PostSeasonPlayerScore.HighestKDA =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Max(x => x.Kda);
            powerRankedPlayer.PostSeasonPlayerScore.HighestKDAHero =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).MaxBy(x => x.Kda)!.HeroId;


            powerRankedPlayer.PostSeasonPlayerScore.ItemTotalBrownBoots =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value)
                    .Count(x =>
                        x.Item0 == DotaEnums.Item.Boots ||
                        x.Item1 == DotaEnums.Item.Boots ||
                        x.Item2 == DotaEnums.Item.Boots ||
                        x.Item3 == DotaEnums.Item.Boots ||
                        x.Item4 == DotaEnums.Item.Boots ||
                        x.Item5 == DotaEnums.Item.Boots);

            powerRankedPlayer.PostSeasonPlayerScore.ItemTotalDivineRapiers =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value)
                    .Count(x =>
                        x.Item0 == DotaEnums.Item.Rapier ||
                        x.Item1 == DotaEnums.Item.Rapier ||
                        x.Item2 == DotaEnums.Item.Rapier ||
                        x.Item3 == DotaEnums.Item.Rapier ||
                        x.Item4 == DotaEnums.Item.Rapier ||
                        x.Item5 == DotaEnums.Item.Rapier);

            powerRankedPlayer.PostSeasonPlayerScore.ItemTotalGemOfTrueSight =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value)
                    .Count(x =>
                        x.Item0 == DotaEnums.Item.Gem ||
                        x.Item1 == DotaEnums.Item.Gem ||
                        x.Item2 == DotaEnums.Item.Gem ||
                        x.Item3 == DotaEnums.Item.Gem ||
                        x.Item4 == DotaEnums.Item.Gem ||
                        x.Item5 == DotaEnums.Item.Gem);
        }


        var heroesMatches = playerMatches.GroupBy(x => x.HeroId).Where(x => x.Any()).ToList();

        foreach (var heroMatches in heroesMatches)
        {
            if (league.LeagueId.HasValue)
            {
                if (powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHero <
                    heroMatches.Count(x => x.LeagueId == league.LeagueId.Value))
                {
                    powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHero =
                        heroMatches.Count(x => x.LeagueId == league.LeagueId.Value);
                    powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHeroId = heroMatches.Key;
                }
            }

            var hero = new PowerRankedHero();
            hero.HeroId = heroMatches.Key;
            hero.LeaderboardRank = player.LeaderboardRank;
            hero.TotalAssists = heroMatches.Sum(x => x.Assists);
            hero.TotalDeaths = heroMatches.Sum(x => x.Deaths);
            hero.TotalKills = heroMatches.Sum(x => x.Kills);
            hero.MatchesPlayed = heroMatches.Count();
            hero.KDA = heroMatches.Average(x => x.Kda);
            hero.MidlanePercent =
                (decimal)heroMatches.Count(x => x.LaneRole == (int)DotaEnums.Lane.Mid && !x.IsRoaming) /
                heroMatches.Count() * 1M;
            hero.SafelanePercent =
                (decimal)heroMatches.Count(x => x.LaneRole == (int)DotaEnums.Lane.Safe && !x.IsRoaming) /
                heroMatches.Count() * 1M;
            hero.JunglePercent =
                (decimal)heroMatches.Count(x => x.LaneRole == (int)DotaEnums.Lane.Jungle && !x.IsRoaming) /
                heroMatches.Count() * 1M;
            hero.OfflanePercent =
                (decimal)heroMatches.Count(x =>
                    x.LaneRole == (int)DotaEnums.Lane.Off && !x.IsRoaming && x.LaneEfficiencyPct >= 40) /
                heroMatches.Count() * 1M;

            hero.SoftSupportPercent = (decimal)heroMatches.Count(x =>
                    x.IsRoaming || (x.LaneRole == (int)DotaEnums.Lane.Off && x.LaneEfficiencyPct < 40)) /
                heroMatches.Count() * 1M;

            hero.HardSupportPercent =
                (decimal)heroMatches.Count(x => x.LaneRole == (int)DotaEnums.Lane.Safe && x.LaneEfficiencyPct < 40) /
                heroMatches.Count() * 1M;

            hero.RoamingPercent =
                (decimal)heroMatches.Count(x => x.IsRoaming) /
                heroMatches.Count() * 1M;
            hero.WinRate =
                (decimal)heroMatches.Count(x => x.Win) /
                heroMatches.Count() * 1M;

            hero.SkillAverageBadge =
                Convert.ToInt32(heroMatches.Where(x => x.RankTier != null && x.RankTier != 0)
                    .Average(x => x.RankTier ?? 0M));

            hero.SoloNormalMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Normal && x.PartySize < 2) /
                heroMatches.Count() * 1M;

            hero.SoloRankedMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Ranked && x.PartySize < 2) /
                heroMatches.Count() * 1M;

            hero.PartyNormalMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Normal && x.PartySize > 1) /
                heroMatches.Count() * 1M;

            hero.PartyRankedMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Ranked && x.PartySize > 1) /
                heroMatches.Count() * 1M;

            hero.BattleCupMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.BattleCup) /
                heroMatches.Count() * 1M;

            hero.LeagueMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Practice) /
                heroMatches.Count() * 1M;

            hero = GetHeroScores(hero);

            powerRankedPlayer.Heroes.Add(hero);
        }

        powerRankedPlayer = GetPlayerScore(powerRankedPlayer);

        return powerRankedPlayer;
    }

    private PowerRankedHero GetHeroScores(PowerRankedHero hero)
    {
        // Ignore heroes who have exceptionally low win rates and matches played
        if (hero.WinRate < DotaRankingConstants.PowerRankWinRateThreshold ||
            hero.MatchesPlayed < DotaRankingConstants.PowerRankGamesPlayedThreshold)
        {
            return hero;
        }

        // Retrieve closest badge weight for the given player hero.
        var badgeWeight = DotaRankingKeyValuePairs.BadgeWeights.MinBy(x =>
            Math.Abs(hero.SkillAverageBadge - (int)x.Key) + DateTime.Now.ToBinary() / 100000000000).Value;

        // Give leaderboard bonus
        if (hero.LeaderboardRank.HasValue)
        {
            var leaderboardBonus = 700 - Convert.ToDecimal(hero.LeaderboardRank) / 6;
            if (leaderboardBonus > 0)
            {
                badgeWeight += leaderboardBonus;
            }
        }

        // Adjust weight based on match types played.
        var lobbyAdjustment =
            hero.SoloRankedMatchMakingPercent * DotaRankingConstants.LobbyWeightSoloRanked +
            hero.SoloNormalMatchMakingPercent * DotaRankingConstants.LobbyWeightSoloNormal +
            hero.PartyNormalMatchMakingPercent * DotaRankingConstants.LobbyWeightPartyNormal +
            hero.PartyRankedMatchMakingPercent * DotaRankingConstants.LobbyWeightPartyRanked +
            hero.BattleCupMatchMakingPercent * DotaRankingConstants.LobbyWeightBattleCup +
            hero.LeagueMatchMakingPercent * DotaRankingConstants.LobbyWeightLeague;

        // Calculate win rate / games played weight.
        var winRateGamesPlayedWeight =
            hero.WinRate * Math.Max(hero.MatchesPlayed, DotaRankingConstants.MaxGamesWeighted) *
            DotaRankingConstants.WinRateGamesPlayedFactor;

        var kdaWeight = Math.Max(hero.KDA, DotaRankingConstants.MaxKDAWeighted) * DotaRankingConstants.KDAFactor;

        hero.Score = (badgeWeight + winRateGamesPlayedWeight + kdaWeight) * lobbyAdjustment;

        hero.ScoreSafelane = hero.Score * hero.SafelanePercent;

        hero.ScoreMidlane = hero.Score * hero.MidlanePercent;

        hero.ScoreOfflane = hero.Score * hero.OfflanePercent;

        hero.ScoreSoftSupport = hero.Score * hero.SoftSupportPercent;

        hero.ScoreHardSupport = hero.Score * hero.HardSupportPercent;

        hero.ScoreRoaming = hero.Score * hero.RoamingPercent;

        hero.ScoreJungle = hero.Score * hero.JunglePercent;

        return hero;
    }

    private PowerRankedPlayer GetPlayerScore(PowerRankedPlayer player)
    {
        // Can't calculate player score if there aren't any heroes with scores
        if (!player.Heroes.Any(x => x.Score > 0))
        {
            return player;
        }

        var safelaneHeroes = player.Heroes.Where(x => x.ScoreSafelane > 0).ToList();
        if (safelaneHeroes.Count > 0)
        {
            player.SafelaneScore = Math.Round(safelaneHeroes.Average(x => x.ScoreSafelane));
        }

        var midLaneHeroes = player.Heroes.Where(x => x.ScoreMidlane > 0).ToList();
        if (midLaneHeroes.Count > 0)
        {
            player.MidlaneScore = Math.Round(midLaneHeroes.Average(x => x.ScoreMidlane));
        }

        var offLaneHeroes = player.Heroes.Where(x => x.ScoreOfflane > 0).ToList();
        if (offLaneHeroes.Count > 0)
        {
            player.OfflaneScore = Math.Round(offLaneHeroes.Average(x => x.ScoreOfflane));
        }

        var softSupportHeroes = player.Heroes.Where(x => x.ScoreSoftSupport > 0).ToList();
        if (softSupportHeroes.Count > 0)
        {
            player.SoftSupportScore = Math.Round(softSupportHeroes.Average(x => x.ScoreSoftSupport));
        }

        var hardSupportHeroes = player.Heroes.Where(x => x.ScoreHardSupport > 0).ToList();
        if (hardSupportHeroes.Count > 0)
        {
            player.HardSupportScore = Math.Round(hardSupportHeroes.Average(x => x.ScoreHardSupport));
        }

        var roamingHeroes = player.Heroes.Where(x => x.ScoreRoaming > 0).ToList();
        if (roamingHeroes.Count > 0)
        {
            player.RoamingScore = Math.Round(roamingHeroes.Average(x => x.ScoreRoaming));
        }

        var jungleHeroes = player.Heroes.Where(x => x.ScoreJungle > 0).ToList();
        if (jungleHeroes.Count > 0)
        {
            player.JungleScore = Math.Round(jungleHeroes.Average(x => x.ScoreJungle));
        }

        player.OverallScore = player.SafelaneScore + player.MidlaneScore + player.OfflaneScore +
                              player.SoftSupportScore + player.HardSupportScore;

        // Calculate toxicity score
        player.ToxicityScore = player.AverageAbandons * DotaRankingConstants.ToxicityAbandonFactor +
                               player.AverageExcessPings * DotaRankingConstants.ToxicityPingFactor +
                               player.AverageExcessPingAbandons * DotaRankingConstants.ToxicityPingAbandonFactor +
                               player.AverageWordToxicity * DotaRankingConstants.ToxicityWordFactor;

        player.RespectBans = GetPlayerHeroRespectBans(player.Heroes).Take(4).ToList();

        return player;
    }

    private decimal GetPlayerAverageWordToxicity(List<PlayerWord> playerWords, int matchCount)
    {
        matchCount = matchCount < 1 ? 1 : matchCount;

        var totalWeights = 0M;
        foreach (var word in PlayerWordWeights.WordWeights)
        {
            var matches = playerWords.Where(x => x.Word.StartsWith(word.Key)).ToList();
            foreach (var match in matches)
            {
                totalWeights += (int)word.Value * match.Count;
            }
        }

        return totalWeights / matchCount;
    }

    private List<DotaEnums.Hero> GetPlayerHeroRespectBans(List<PowerRankedHero> heroes)
    {
        return heroes.Where(x => x.MatchesPlayed >= DotaRankingConstants.PowerRankRespectBansThreshold)
            .OrderByDescending(x => x.Score).Select(x => x.HeroId).ToList();
    }

    private PowerRankedTeam GetTeamRoles(PowerRankedTeam team)
    {
        var safeLanePlayer = team.Players.MaxBy(x => x.SafelaneScore);
        if (safeLanePlayer != null)
        {
            safeLanePlayer.TeamRole = DotaEnums.TeamRole.Safelane;
        }

        var midLanePlayer = team.Players.Where(x => x.TeamRole == null).MaxBy(x => x.MidlaneScore);
        if (midLanePlayer != null)
        {
            midLanePlayer.TeamRole = DotaEnums.TeamRole.Midlane;
        }

        var offLanePlayer = team.Players.Where(x => x.TeamRole == null).MaxBy(x => x.OfflaneScore);
        if (offLanePlayer != null)
        {
            offLanePlayer.TeamRole = DotaEnums.TeamRole.Offlane;
        }

        var softSupportPlayer = team.Players.Where(x => x.TeamRole == null).MaxBy(x => x.SoftSupportScore);
        if (softSupportPlayer != null)
        {
            softSupportPlayer.TeamRole = DotaEnums.TeamRole.SoftSupport;
        }

        var hardSupportPlayer = team.Players.Where(x => x.TeamRole == null).MaxBy(x => x.HardSupportScore);
        if (hardSupportPlayer != null)
        {
            hardSupportPlayer.TeamRole = DotaEnums.TeamRole.HardSupport;
        }

        team.TotalScore =
            (team.Players.FirstOrDefault(x => x.TeamRole == DotaEnums.TeamRole.Safelane)?.SafelaneScore ?? 0) *
            DotaRankingConstants.TeamSafelaneScoreFactor +
            (team.Players.FirstOrDefault(x => x.TeamRole == DotaEnums.TeamRole.Midlane)?.MidlaneScore ?? 0) *
            DotaRankingConstants.TeamMidlaneScoreFactor +
            (team.Players.FirstOrDefault(x => x.TeamRole == DotaEnums.TeamRole.Offlane)?.OfflaneScore ?? 0) *
            DotaRankingConstants.TeamOfflaneScoreFactor +
            (team.Players.FirstOrDefault(x => x.TeamRole == DotaEnums.TeamRole.SoftSupport)?.SoftSupportScore ?? 0) *
            DotaRankingConstants.TeamSoftSupportScoreFactor +
            (team.Players.FirstOrDefault(x => x.TeamRole == DotaEnums.TeamRole.HardSupport)?.HardSupportScore ?? 0) *
            DotaRankingConstants.TeamHardSupportScoreFactor;

        team.BadgeAverage = Convert.ToInt32(team.Players.Average(x => x.RankTier));

        team.EstimatedMMRAverage = team.Players.Average(x => x.MmrEstimate ?? 0M);

        return team;
    }


    public List<PowerRankedPlayer> RankPlayers(List<PowerRankedPlayer> players, PowerRankedDivision division)
    {
        foreach (var player in players)
        {
            player.SafelaneRank = players.ToList()
                .OrderByDescending(x => x.SafelaneScore)
                .ToList().IndexOf(player) + 1;

            player.MidlaneRank = players
                .OrderByDescending(x => x.MidlaneScore)
                .ToList().IndexOf(player) + 1;

            player.OfflaneRank = players
                .OrderByDescending(x => x.OfflaneScore)
                .ToList().IndexOf(player) + 1;

            player.SoftSupportRank = players
                .OrderByDescending(x => x.SoftSupportScore)
                .ToList().IndexOf(player) + 1;

            player.HardSupportRank = players
                .OrderByDescending(x => x.HardSupportScore)
                .ToList().IndexOf(player) + 1;

            player.RoamingRank = players
                .OrderByDescending(x => x.RoamingScore)
                .ToList().IndexOf(player) + 1;

            player.JungleRank = players
                .OrderByDescending(x => x.JungleScore)
                .ToList().IndexOf(player) + 1;

            player.OverallRank = players
                .OrderByDescending(x => x.OverallScore)
                .ToList().IndexOf(player) + 1;

            player.ToxicityRank = players
                .OrderByDescending(x => x.ToxicityScore)
                .ToList().IndexOf(player) + 1;


            /*player.SafelaneDivisionRank = players
                .Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.SafelaneScore)
                .ToList().IndexOf(player) + 1;

            player.MidlaneDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.MidlaneScore)
                .ToList().IndexOf(player) + 1;

            player.OfflaneDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.OfflaneScore)
                .ToList().IndexOf(player) + 1;

            player.SoftSupportDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.SoftSupportScore)
                .ToList().IndexOf(player) + 1;

            player.HardSupportDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.HardSupportScore)
                .ToList().IndexOf(player) + 1;

            player.RoamingDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.RoamingScore)
                .ToList().IndexOf(player) + 1;

            player.JungleDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.JungleScore)
                .ToList().IndexOf(player) + 1;

            player.OverallDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.OverallScore)
                .ToList().IndexOf(player) + 1;

            player.ToxicityDivisionRank = players.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.ToxicityScore)
                .ToList().IndexOf(player) + 1;*/
        }

        return players.Where(x => x.DivisionName == division.Name).ToList();
    }

    private PowerRankedTeam MapTeamSheetToTeam(PowerRankedTeam powerRankedTeam, PlayerDataSourceTeam sheetTeam)
    {
        var captain = sheetTeam.Players.First(x => x.IsCaptain);
        powerRankedTeam.Players.First(x => x.PlayerId == captain.Id).IsCaptain = true;
        powerRankedTeam.Name = captain.Name;

        powerRankedTeam.Players.ForEach(x =>
        {
            x.TeamName = captain.Name;
            x.DivisionName = powerRankedTeam.DivisionName;
        });

        return powerRankedTeam;
    }

    private PowerRankedLeague RankTeams(PowerRankedLeague league)
    {
        foreach (var team in league.Divisions.SelectMany(x => x.Teams))
        {
            team.Rank = league.Divisions.SelectMany(x => x.Teams).OrderByDescending(x => x.TotalScore).ToList()
                .IndexOf(team) + 1;

            team.DivisionRank = league.Divisions
                .SelectMany(x => x.Teams.Where(y => y.DivisionName == team.DivisionName))
                .OrderByDescending(x => x.TotalScore).ToList()
                .IndexOf(team) + 1;
        }

        return league;
    }
}