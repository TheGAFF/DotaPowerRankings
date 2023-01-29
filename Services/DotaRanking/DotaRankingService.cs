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
using RD2LPowerRankings.Services.PostSeasonAwards;

namespace RD2LPowerRankings.Services.DotaRanking;

public class DotaRankingService : IDotaRankingService
{
    private readonly DotaDbContext _context;
    private readonly ILogger<DotaRankingService> _logger;
    private readonly IMapper _mapper;
    private readonly IPlayerDataSource _playerDataSource;
    private readonly IPostSeasonAwardService _postSeasonAwardService;
    private readonly IDotaAwardsService _dotaAwardsService;
    private readonly IOpenAIService _openAiService;

    public DotaRankingService(ILogger<DotaRankingService> logger, DotaDbContext context, IMapper mapper,
        IPlayerDataSource playerDataSource, IPostSeasonAwardService postSeasonAwardService,
        IDotaAwardsService dotaAwardsService, IOpenAIService openAiService)
    {
        _logger = logger;
        _context = context;
        _mapper = mapper;
        _playerDataSource = playerDataSource;
        _postSeasonAwardService = postSeasonAwardService;
        _dotaAwardsService = dotaAwardsService;
        _openAiService = openAiService;
    }


    public async Task<PowerRankedLeague> GeneratePreSeasonLeaguePowerRankings(PlayerDataSourceLeague league)
    {
        var powerRankedLeague = new PowerRankedLeague { Name = league.Name, LeagueId = league.LeagueId };

        foreach (var division in league.Divisions)
        {
            var powerRankedDivision = new PowerRankedDivision { Name = division.Name };

            var playerIds = _playerDataSource.GetPlayers(division.SheetId).Select(x => x.Id).ToArray();

            powerRankedDivision.Players = GeneratePlayersStats(playerIds, powerRankedLeague, powerRankedDivision);

            powerRankedDivision.Players = _dotaAwardsService.GiveDivisionPlayerAwards(powerRankedDivision.Players);

            powerRankedDivision.Players = RankPlayers(powerRankedDivision.Players, powerRankedDivision);

            powerRankedDivision.Players = await _openAiService.GeneratePlayerReviews(powerRankedDivision.Players);

            powerRankedLeague.Divisions.Add(powerRankedDivision);
        }


        return powerRankedLeague;
    }


    public async Task<PowerRankedLeague> GeneratePostSeasonLeaguePowerRankings(PlayerDataSourceLeague league)
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

            RankPlayers(powerRankedDivision.Teams.SelectMany(x => x.Players).ToList(),
                powerRankedDivision);

            _dotaAwardsService.GiveDivisionPlayerAwards(powerRankedDivision.Teams.SelectMany(x => x.Players).ToList());

            powerRankedDivision = _dotaAwardsService.GiveDivisionTeamAwards(powerRankedDivision);

            powerRankedLeague.Divisions.Add(powerRankedDivision);
        }


        powerRankedLeague = RankTeams(powerRankedLeague);

        powerRankedLeague = _postSeasonAwardService.GeneratePostSeasonAwards(powerRankedLeague);

        foreach (var division in powerRankedLeague.Divisions)
        {
            foreach (var team in division.Teams)
            {
                team.Players = await _openAiService.GeneratePlayerReviews(team.Players);
            }

            division.Teams =
                await _openAiService.GenerateTeamReviews(division.Teams,
                    $"{division.Name}-{league.Name}-{league.FileName}");
        }


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
        var playerWords = _context.PlayerWords.AsNoTracking().Where(x => playerIds.Any(y => y == x.PlayerId)).ToList();
        var playerItemUses = _context.PlayerMatchItemUses.AsNoTracking()
            .Where(x => playerIds.Any(y => y == x.PlayerId)).ToList();
        var playerAbilities = _context.PlayerMatchAbilities.AsNoTracking()
            .Where(x => playerIds.Any(y => y == x.PlayerId)).ToList();
        Parallel.ForEach(playerIds, new ParallelOptions { MaxDegreeOfParallelism = 4 },
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
                        playerItemUses.Where(x => x.PlayerId == playerId).ToList(),
                        playerAbilities.Where(x => x.PlayerId == playerId).ToList(),
                        league,
                        division
                    ));
            });

        return powerRankedPlayers.ToList();
    }

    private PowerRankedPlayer GeneratePowerRankedPlayer(Player player,
        List<PlayerMatch> playerMatches,
        List<PlayerWord> playerWords,
        List<PlayerMatchItemUse> playerItemUses,
        List<PlayerMatchAbility> playerMatchAbilities,
        PowerRankedLeague league,
        PowerRankedDivision division)
    {
        foreach (var playerMatch in playerMatches)
        {
            playerMatch.Match.PlayerMatchItemUses =
                playerItemUses.Where(x => x.MatchId == playerMatch.MatchId).ToList();
            playerMatch.Match.PlayerMatchAbilities =
                playerMatchAbilities.Where(x => x.MatchId == playerMatch.MatchId).ToList();
        }


        var powerRankedPlayer = _mapper.Map<PowerRankedPlayer>(player);
        powerRankedPlayer.DivisionName = division.Name;

        if (playerMatches.Count == 0)
        {
            return powerRankedPlayer;
        }

        powerRankedPlayer.AverageWordToxicity = GetPlayerAverageWordToxicity(playerWords, playerMatches.Count);

        powerRankedPlayer.FirstBloodAverage =
            (decimal)playerMatches.Count(x => x.FirstbloodClaimed) / playerMatches.Count * 1M;

        powerRankedPlayer.CourierKillAverage =
            (decimal)playerMatches.Sum(x => x.CourierKills) / playerMatches.Count;

        powerRankedPlayer.CampsStackedAverage =
            (decimal)playerMatches.Sum(x => x.CampsStacked) / playerMatches.Count;

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
            playerMatches.Count(x => x.LeaverStatus == "ABANDONED") * 1M / playerMatches.Count * 1M;

        powerRankedPlayer.AverageExcessPings = (decimal)playerMatches
            .Where(x => x.Pings > DotaRankingConstants.ExcessivePingThreshold)
            .Sum(x => x.Pings) / playerMatches.Count;

        powerRankedPlayer.AverageExcessPingAbandons =
            (decimal)playerMatches.Where(x => x.Pings > DotaRankingConstants.ExcessivePingThreshold && x.LeaverStatus ==
                    "ABANDONED")
                .Sum(x => x.Pings) / playerMatches.Count;

        powerRankedPlayer.AverageTeamFightParticipation = playerMatches.Average(x => x.TeamfightParticipation);
        powerRankedPlayer.AverageStuns = playerMatches.Average(x => x.StunDuration * 1M);
        powerRankedPlayer.AverageAPM = (decimal)playerMatches.Average(x => x.ActionsPerMinute);
        powerRankedPlayer.AverageTowerDamage = (decimal)playerMatches.Average(x => x.TowerDamage);

        powerRankedPlayer.AverageLaneEfficiency =
            (decimal)playerMatches.Where(x => x.LaneEfficiencyPct > 40).Average(x => x.LaneEfficiencyPct) / 4000M;

        powerRankedPlayer.AverageLaneEfficiencyMid =
            (decimal)playerMatches.Where(x => x.LaneRole == DotaEnums.TeamRole.Midlane && x.Lane == DotaEnums.Lane.Mid)
                .DefaultIfEmpty()
                .Average(x => x?.LaneEfficiencyPct ?? 0) / 4000M;

        powerRankedPlayer.AverageLaneEfficiencyOff =
            (decimal)playerMatches.Where(x => x.LaneRole == DotaEnums.TeamRole.Offlane && x.Lane == DotaEnums.Lane.Off)
                .DefaultIfEmpty()
                .Average(x => x?.LaneEfficiencyPct ?? 0) / 4000M;

        powerRankedPlayer.AverageLaneEfficiencySafe =
            (decimal)playerMatches
                .Where(x => x.LaneRole == DotaEnums.TeamRole.Safelane && x.Lane == DotaEnums.Lane.Safe)
                .DefaultIfEmpty()
                .Average(x => x?.LaneEfficiencyPct ?? 0) / 4000M;

        powerRankedPlayer.AverageSentriesPlaced =
            (decimal)playerMatches.Where(x => x.LaneEfficiencyPct < 40).Select(x => x.SenPlaced).DefaultIfEmpty(0)
                .Average();

        powerRankedPlayer.AverageArmletToggles =
            (decimal)playerMatches.Average(x =>
                x.Match.PlayerMatchItemUses.Where(y => y.ItemId == (int)DotaEnums.Item.Armlet).Sum(y => y.Uses));


        powerRankedPlayer.AverageGankKills = playerMatches.Average(x => x.GankKillCount * 1M);

        powerRankedPlayer.AverageSmokeKills = playerMatches.Average(x => x.SmokeKillCount * 1M);

        powerRankedPlayer.AverageSoloKills = playerMatches.Average(x => x.SoloKillCount * 1M);

        powerRankedPlayer.AverageTpKills = playerMatches.Average(x => x.TpRecentlyKillCount * 1M);

        powerRankedPlayer.AverageInvisibleKills = playerMatches.Average(x => x.InvisibleKillCount * 1M);

        powerRankedPlayer.AverageScans = playerMatches.Average(x => (x.Scans ?? 0) * 1M);

        powerRankedPlayer.AverageIntentionalFeeding =
            playerMatches.Count(x => x.IntentionalFeeding) * 1M / playerMatches.Count * 1M;

        powerRankedPlayer.AverageMVPs =
            playerMatches.Count(x => x.Award == "MVP") * 1M / playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestCore =
            playerMatches.Count(x => x.Award == "TOP_CORE" && (x.LaneRole == DotaEnums.TeamRole.Safelane ||
                                                               x.LaneRole == DotaEnums.TeamRole.Offlane ||
                                                               x.LaneRole == DotaEnums.TeamRole.Midlane)) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestPos1 =
            playerMatches.Count(x => x.Award == "TOP_CORE" && x.LaneRole == DotaEnums.TeamRole.Safelane) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestPos2 =
            playerMatches.Count(x => x.Award == "TOP_CORE" && x.LaneRole == DotaEnums.TeamRole.Midlane) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestPos3 =
            playerMatches.Count(x => x.Award == "TOP_CORE" && x.LaneRole == DotaEnums.TeamRole.Offlane) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestSupport =
            playerMatches.Count(x => x.Award == "TOP_SUPPORT" && (x.LaneRole == DotaEnums.TeamRole.HardSupport ||
                                                                  x.LaneRole == DotaEnums.TeamRole.SoftSupport)) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestPos4 =
            playerMatches.Count(x => x.Award == "TOP_SUPPORT" && x.LaneRole == DotaEnums.TeamRole.SoftSupport) * 1M /
            playerMatches.Count * 1M;

        powerRankedPlayer.AverageBestPos5 =
            playerMatches.Count(x => x.Award == "TOP_SUPPORT" && x.LaneRole == DotaEnums.TeamRole.HardSupport) * 1M /
            playerMatches.Count * 1M;


        powerRankedPlayer.AveragePauses = playerMatches.Average(x => x.PauseCount * 1M);

        powerRankedPlayer.AverageRandomHeroes = playerMatches.Count(x => x.Randomed) * 1M / playerMatches.Count * 1M;

        powerRankedPlayer =
            _postSeasonAwardService.CalculatePostSeasonPlayerScore(powerRankedPlayer, league, playerMatches);

        var heroesMatches = playerMatches.GroupBy(x => x.HeroId).Where(x => x.Any()).ToList();

        foreach (var heroMatches in heroesMatches)
        {
            powerRankedPlayer =
                _postSeasonAwardService.CalculatePostSeasonHeroScore(powerRankedPlayer, league, heroMatches);


            var hero = new PowerRankedHero();
            hero.HeroId = heroMatches.Key;
            hero.LeaderboardRank = player.LeaderboardRank;
            hero.TotalAssists = heroMatches.Sum(x => x.Assists);
            hero.TotalDeaths = heroMatches.Sum(x => x.Deaths);
            hero.TotalKills = heroMatches.Sum(x => x.Kills);
            hero.MatchesPlayed = heroMatches.Count();
            hero.KDA = heroMatches.Average(x => x.Kda);
            hero.MidlanePercent =
                (decimal)heroMatches.Count(x => x.Lane == DotaEnums.Lane.Mid) /
                heroMatches.Count() * 1M;
            hero.SafelanePercent =
                (decimal)heroMatches.Count(x =>
                    x.Lane == DotaEnums.Lane.Safe && x.LaneRole == DotaEnums.TeamRole.Safelane) /
                heroMatches.Count() * 1M;
            hero.JunglePercent =
                (decimal)heroMatches.Count(x => x.Lane == DotaEnums.Lane.Jungle) /
                heroMatches.Count() * 1M;
            hero.OfflanePercent =
                (decimal)heroMatches.Count(x =>
                    x.Lane == DotaEnums.Lane.Off && x.LaneRole == DotaEnums.TeamRole.Offlane) /
                heroMatches.Count() * 1M;

            hero.SoftSupportPercent = (decimal)heroMatches.Count(x =>
                    x.LaneRole == DotaEnums.TeamRole.SoftSupport && x.Lane == DotaEnums.Lane.Off) /
                heroMatches.Count() * 1M;

            hero.HardSupportPercent =
                (decimal)heroMatches.Count(x => x.LaneRole == DotaEnums.TeamRole.HardSupport) /
                heroMatches.Count() * 1M;

            hero.RoamingPercent =
                (decimal)heroMatches.Count(x => x.IsRoaming) /
                heroMatches.Count() * 1M;
            hero.WinRate =
                (decimal)heroMatches.Count(x => x.Win) /
                heroMatches.Count() * 1M;

            hero.SkillAverageBadge =
                Convert.ToInt32(heroMatches.Where(x => x.MatchRank != null && x.MatchRank != 0)
                    .Average(x => x.MatchRank ?? 0M));

            hero.SoloNormalMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Normal && x.PartyId == null) /
                heroMatches.Count() * 1M;

            hero.SoloRankedMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Ranked && x.PartyId == null) /
                heroMatches.Count() * 1M;

            hero.PartyNormalMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Normal && x.PartyId != null) /
                heroMatches.Count() * 1M;

            hero.PartyRankedMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Ranked && x.PartyId != null) /
                heroMatches.Count() * 1M;

            hero.BattleCupMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.BattleCup) /
                heroMatches.Count() * 1M;

            hero.LeagueMatchMakingPercent =
                (decimal)heroMatches.Count(x => x.LobbyType == DotaEnums.LobbyType.Practice) /
                heroMatches.Count() * 1M;

            hero.Impact = heroMatches.Average(x => x.Imp * 1M);

            hero = GetHeroScores(hero);

            powerRankedPlayer.Heroes.Add(hero);
        }

        powerRankedPlayer = GetPlayerScore(powerRankedPlayer);

        return powerRankedPlayer;
    }

    private static PowerRankedHero GetHeroScores(PowerRankedHero hero)
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

        var impactWeight = hero.Impact * DotaRankingConstants.ImpactFactor;

        hero.Score = (kdaWeight + impactWeight) * lobbyAdjustment;

        hero.MinimumScore = badgeWeight * lobbyAdjustment;

        hero.TotalScore = hero.Score + hero.MinimumScore;

        hero.ScoreSafelane = hero.Score * hero.SafelanePercent;

        hero.ScoreMidlane = hero.Score * hero.MidlanePercent;

        hero.ScoreOfflane = hero.Score * hero.OfflanePercent;

        hero.ScoreSoftSupport = hero.Score * hero.SoftSupportPercent;

        hero.ScoreHardSupport = hero.Score * hero.HardSupportPercent;

        hero.ScoreRoaming = hero.Score * hero.RoamingPercent;

        hero.ScoreJungle = hero.Score * hero.JunglePercent;

        return hero;
    }

    private static PowerRankedPlayer GetPlayerScore(PowerRankedPlayer player)
    {
        // Can't calculate player score if there aren't any heroes with scores
        if (!player.Heroes.Any(x => x.TotalScore > 0))
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

        var minimumScore = player.Heroes.Average(x => x.MinimumScore * 5M);

        player.OverallScore = minimumScore + player.SafelaneScore + player.MidlaneScore + player.OfflaneScore +
                              player.SoftSupportScore + player.HardSupportScore;

        // Calculate toxicity score
        player.ToxicityScore = player.AverageAbandons * DotaRankingConstants.ToxicityAbandonFactor +
                               player.AverageExcessPings * DotaRankingConstants.ToxicityPingFactor +
                               player.AverageExcessPingAbandons * DotaRankingConstants.ToxicityPingAbandonFactor +
                               player.AverageWordToxicity * DotaRankingConstants.ToxicityWordFactor +
                               player.AverageIntentionalFeeding * DotaRankingConstants.ToxicityIntentionalFeedingFactor;

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

    private static IEnumerable<DotaEnums.Hero> GetPlayerHeroRespectBans(List<PowerRankedHero> heroes)
    {
        return heroes.Where(x => x.MatchesPlayed >= DotaRankingConstants.PowerRankRespectBansThreshold)
            .OrderByDescending(x => x.Score).Select(x => x.HeroId);
    }

    private static PowerRankedTeam GetTeamRoles(PowerRankedTeam team)
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


    private static List<PowerRankedPlayer> RankPlayers(List<PowerRankedPlayer> players,
        PowerRankedDivision division)
    {
        var powerRankedPlayers = players.ToList();
        foreach (var player in powerRankedPlayers)
        {
            player.SafelaneRank = powerRankedPlayers.ToList()
                .OrderByDescending(x => x.SafelaneScore)
                .ToList().IndexOf(player) + 1;

            player.MidlaneRank = powerRankedPlayers
                .OrderByDescending(x => x.MidlaneScore)
                .ToList().IndexOf(player) + 1;

            player.OfflaneRank = powerRankedPlayers
                .OrderByDescending(x => x.OfflaneScore)
                .ToList().IndexOf(player) + 1;

            player.SoftSupportRank = powerRankedPlayers
                .OrderByDescending(x => x.SoftSupportScore)
                .ToList().IndexOf(player) + 1;

            player.HardSupportRank = powerRankedPlayers
                .OrderByDescending(x => x.HardSupportScore)
                .ToList().IndexOf(player) + 1;

            player.RoamingRank = powerRankedPlayers
                .OrderByDescending(x => x.RoamingScore)
                .ToList().IndexOf(player) + 1;

            player.JungleRank = powerRankedPlayers
                .OrderByDescending(x => x.JungleScore)
                .ToList().IndexOf(player) + 1;

            player.OverallRank = powerRankedPlayers
                .OrderByDescending(x => x.OverallScore)
                .ToList().IndexOf(player) + 1;

            player.ToxicityRank = powerRankedPlayers
                .OrderByDescending(x => x.ToxicityScore)
                .ToList().IndexOf(player) + 1;

            player.SafelaneDivisionRank = powerRankedPlayers
                .Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.SafelaneScore)
                .ToList().IndexOf(player) + 1;

            player.MidlaneDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.MidlaneScore)
                .ToList().IndexOf(player) + 1;

            player.OfflaneDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.OfflaneScore)
                .ToList().IndexOf(player) + 1;

            player.SoftSupportDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.SoftSupportScore)
                .ToList().IndexOf(player) + 1;

            player.HardSupportDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.HardSupportScore)
                .ToList().IndexOf(player) + 1;

            player.RoamingDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.RoamingScore)
                .ToList().IndexOf(player) + 1;

            player.JungleDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.JungleScore)
                .ToList().IndexOf(player) + 1;

            player.OverallDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.OverallScore)
                .ToList().IndexOf(player) + 1;

            player.ToxicityDivisionRank = powerRankedPlayers.Where(z => z.DivisionName == player.DivisionName)
                .OrderByDescending(x => x.ToxicityScore)
                .ToList().IndexOf(player) + 1;
        }

        return powerRankedPlayers.Where(x => x.DivisionName == division.Name).ToList();
    }

    private static PowerRankedTeam MapTeamSheetToTeam(PowerRankedTeam powerRankedTeam, PlayerDataSourceTeam sheetTeam)
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

    private static PowerRankedLeague RankTeams(PowerRankedLeague league)
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