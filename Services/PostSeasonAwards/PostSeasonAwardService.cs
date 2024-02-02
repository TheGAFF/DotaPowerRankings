using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.DotaRanking.Enums;
using RD2LPowerRankings.Services.PostSeasonAwards;

namespace RD2LPowerRankings.Services.DotaRanking;

public class PostSeasonAwardService : IPostSeasonAwardService
{
    private readonly DotaDbContext _context;
    private readonly ILogger<PostSeasonAwardService> _logger;

    public PostSeasonAwardService(ILogger<PostSeasonAwardService> logger, DotaDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public PowerRankedLeague GeneratePostSeasonAwards(PowerRankedLeague league)
    {
        if (!league.LeagueId.HasValue)
        {
            return league;
        }

        foreach (var division in league.Divisions)
        {
            division.PostSeasonAwards = GivePostSeasonAwards(division);
        }


        return league;
    }

    public PowerRankedPlayer CalculatePostSeasonPlayerScore(PowerRankedPlayer powerRankedPlayer,
        PowerRankedLeague league, List<PlayerMatch> playerMatches)
    {
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
            powerRankedPlayer.PostSeasonPlayerScore.TotalCampsStacked =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.CampsStacked);
            powerRankedPlayer.PostSeasonPlayerScore.TotalCourierKills =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.CourierKills);
            powerRankedPlayer.PostSeasonPlayerScore.TotalFirstBloods =
                playerMatches.Count(x => x.LeagueId == league.LeagueId.Value && x.FirstbloodClaimed);
            powerRankedPlayer.PostSeasonPlayerScore.TotalLastHits =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.LastHits);
            powerRankedPlayer.PostSeasonPlayerScore.TotalObsPlaced =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.ObsPlaced);
            powerRankedPlayer.PostSeasonPlayerScore.TotalRunesPickedUp =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.RunePickups);
            powerRankedPlayer.PostSeasonPlayerScore.TotalSentriesPlaced =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.SenPlaced);
            powerRankedPlayer.PostSeasonPlayerScore.TotalStunSeconds =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.StunDuration);
            powerRankedPlayer.PostSeasonPlayerScore.TotalTimeDead =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.LifeStateDead);
            powerRankedPlayer.PostSeasonPlayerScore.TotalTowerDamage =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.TowerDamage);
            powerRankedPlayer.PostSeasonPlayerScore.AverageActionsPerMin =
                (decimal)playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.ActionsPerMinute) /
                powerRankedPlayer.PostSeasonPlayerScore.TotalGames * 1M;
            powerRankedPlayer.PostSeasonPlayerScore.AverageTeamFightParticipation = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.TeamfightParticipation);

            powerRankedPlayer.PostSeasonPlayerScore.AverageLaneEfficiencyPct =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.LaneEfficiencyPct * 1M) /
                4000M;

            powerRankedPlayer.PostSeasonPlayerScore.TotalHeroesPlayed =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).GroupBy(x => x.HeroId).Count();

            var matches = playerMatches.Where(x => x.LeagueId == league.LeagueId.Value && x.Win).ToList();
            if (matches.Any())
            {
                powerRankedPlayer.PostSeasonPlayerScore.LongestWonGameLength = matches.Max(x => x.Duration);
            }

            matches = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value && x.Win && x.Duration > 0).ToList();
            powerRankedPlayer.PostSeasonPlayerScore.ShortestWonGameLength =
                matches.Any() ? matches.Min(x => x.Duration) : long.MaxValue;

            powerRankedPlayer.PostSeasonPlayerScore.HighestKDA =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Max(x => x.Kda);
            powerRankedPlayer.PostSeasonPlayerScore.HighestKDAHero =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).MaxBy(x => x.Kda)!.HeroId;


            powerRankedPlayer.PostSeasonPlayerScore.LowestKDA =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Min(x => x.Kda);
            powerRankedPlayer.PostSeasonPlayerScore.LowestKDAHero =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).MinBy(x => x.Kda)!.HeroId;

            powerRankedPlayer.PostSeasonPlayerScore.KDAAverage =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.Kda);

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

            powerRankedPlayer.PostSeasonPlayerScore.TotalSmokesUsed =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value)
                    .Sum(x =>
                        x.Match.PlayerMatchItemUses.Count(y =>
                            y.PlayerId == powerRankedPlayer.PlayerId && y.ItemId == (int)DotaEnums.Item.SmokeOfDeceit));

            powerRankedPlayer.PostSeasonPlayerScore.TotalDustUsed =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value)
                    .Sum(x =>
                        x.Match.PlayerMatchItemUses.Count(y =>
                            y.PlayerId == powerRankedPlayer.PlayerId && y.ItemId == (int)DotaEnums.Item.Dust));

            powerRankedPlayer.PostSeasonPlayerScore.DeathsWhileTpingCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.DeathTpAttemptCount);

            powerRankedPlayer.PostSeasonPlayerScore.PauseCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.PauseCount);

            powerRankedPlayer.PostSeasonPlayerScore.SoloKillCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.SoloKillCount);

            powerRankedPlayer.PostSeasonPlayerScore.SmokeKillCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.SmokeKillCount);

            powerRankedPlayer.PostSeasonPlayerScore.GankKillCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Sum(x => x.GankKillCount);

            powerRankedPlayer.PostSeasonPlayerScore.AverageImpact = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.Imp * 1M);

            powerRankedPlayer.PostSeasonPlayerScore.BestCoreOfGameCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Count(x => x.Award == "TOP_CORE");

            powerRankedPlayer.PostSeasonPlayerScore.BestSupportOfGameCount = playerMatches
                .Where(x => x.LeagueId == league.LeagueId.Value).Count(x => x.Award == "TOP_SUPPORT");

            powerRankedPlayer.PostSeasonPlayerScore.TotalAvgGoldFed =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.GoldFed * 1M);

            powerRankedPlayer.PostSeasonPlayerScore.TotalAvgGoldLost =
                playerMatches.Where(x => x.LeagueId == league.LeagueId.Value).Average(x => x.GoldLost * 1M);

            powerRankedPlayer.PostSeasonPlayerScore.TotalSupportGoldSpent =
                powerRankedPlayer.PostSeasonPlayerScore.TotalDustUsed * 80 +
                powerRankedPlayer.PostSeasonPlayerScore.TotalSmokesUsed * 50 +
                powerRankedPlayer.PostSeasonPlayerScore.TotalSentriesPlaced * 50 +
                powerRankedPlayer.PostSeasonPlayerScore.ItemTotalGemOfTrueSight * 900;
        }

        return powerRankedPlayer;
    }

    public PowerRankedPlayer CalculatePostSeasonHeroScore(PowerRankedPlayer powerRankedPlayer, PowerRankedLeague league,
        IGrouping<DotaEnums.Hero, PlayerMatch> heroMatches, DayOfWeek leagueDay)
    {
        if (league.LeagueId.HasValue)
        {
            if (powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHero <
                heroMatches.Count(x => x.LeagueId == league.LeagueId.Value &&
                                       DateTimeOffset.FromUnixTimeSeconds(x.StartTime).DayOfWeek ==
                                       leagueDay))
            {
                powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHero =
                    heroMatches.Count(x => x.LeagueId == league.LeagueId.Value &&
                                           DateTimeOffset.FromUnixTimeSeconds(x.StartTime).DayOfWeek ==
                                           leagueDay);
                powerRankedPlayer.PostSeasonPlayerScore.MostGamesOnSingleHeroId = heroMatches.Key;
            }
        }

        return powerRankedPlayer;
    }

    private List<PostSeasonAward> GivePostSeasonAwards(PowerRankedDivision division)
    {
        var awards = new List<PostSeasonAward>();

        var players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalAssists).Take(2).ToArray();
        var award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Most Assists",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalAssists:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalAssists:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players).OrderByDescending(x => x.PostSeasonPlayerScore.TotalDenies)
            .Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Denies",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalDenies:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalDenies:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players).OrderByDescending(x => x.PostSeasonPlayerScore.TotalGold)
            .Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Most Gold Collected",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalGold:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalGold:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players).OrderByDescending(x => x.PostSeasonPlayerScore.TotalHealing)
            .Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Most Healing",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalHealing:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalHealing:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players).OrderByDescending(x => x.PostSeasonPlayerScore.TotalPings)
            .Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Pings",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalPings:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalPings:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalCampsStacked).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Most Camps Stacked",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalCampsStacked:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalCampsStacked:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalCourierKills).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Most Couriers Killed",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalCourierKills:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalCourierKills:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.BestSupportOfGameCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Best Support Overall",
            Awardee = players[0].DraftName!,
            RunnerUp = players[1].DraftName!
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalFirstBloods).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total First Bloods",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalFirstBloods:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalFirstBloods:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalLastHits).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Total Last Hits",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalLastHits:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalLastHits:N0}"
        };
        awards.Add(award);


        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalObsPlaced).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Total Observers Placed",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalObsPlaced}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalObsPlaced}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalSentriesPlaced).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Total Sentries Placed",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalSentriesPlaced:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalSentriesPlaced:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalSmokesUsed).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Total Smokes Used",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalSmokesUsed:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalSmokesUsed:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalDustUsed).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Total Dust Used",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalDustUsed:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalDustUsed:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalSupportGoldSpent).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Support,
            Name = "Total Support Gold Spent",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalSupportGoldSpent:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalSupportGoldSpent:N0}"
        };
        awards.Add(award);


        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalTimeDead).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Time Dead (minutes)",
            Awardee = players[0].DraftName!,
            Value = $"{TimeSpan.FromSeconds(players[0].PostSeasonPlayerScore.TotalTimeDead).TotalMinutes:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{TimeSpan.FromSeconds(players[1].PostSeasonPlayerScore.TotalTimeDead).TotalMinutes:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalStunSeconds).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Stuns (Seconds)",
            Awardee = players[0].DraftName!,
            Value = $"{TimeSpan.FromMilliseconds(players[0].PostSeasonPlayerScore.TotalStunSeconds).TotalSeconds:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{TimeSpan.FromMilliseconds(players[1].PostSeasonPlayerScore.TotalStunSeconds).TotalSeconds:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalTowerDamage).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Tower Damage",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalTowerDamage:N0}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalTowerDamage:N0}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.AverageActionsPerMin).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Highest Average Actions Per Minute",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.AverageActionsPerMin:0.00}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.AverageActionsPerMin:0.00}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.AverageTeamFightParticipation).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Highest Average Team Fight Participation %",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.AverageTeamFightParticipation:0.00}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.AverageTeamFightParticipation:0.00}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.AverageLaneEfficiencyPct).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Highest Average Lane Efficiency %",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.AverageLaneEfficiencyPct:0.00}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.AverageLaneEfficiencyPct:0.00}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.SoloKillCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Most Solo Kills",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.SoloKillCount}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.SoloKillCount}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.GankKillCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Most Gank Kills",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.GankKillCount}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.GankKillCount}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.BestCoreOfGameCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Core,
            Name = "Best Overall Core",
            Awardee = players[0].DraftName!,
            RunnerUp = players[1].DraftName!
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.TotalRunesPickedUp).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Runes Picked Up",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.TotalRunesPickedUp}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.TotalRunesPickedUp}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.SmokeKillCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Smoke Kill Count",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.SmokeKillCount}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.SoloKillCount}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.PauseCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Pauses",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.PauseCount}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.PauseCount}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.DeathsWhileTpingCount).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Deaths while TPing Out",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.DeathsWhileTpingCount}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.DeathsWhileTpingCount}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.MostGamesOnSingleHero).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Games on a Single Hero",
            Awardee = players[0].DraftName!,
            Value =
                $"{players[0].PostSeasonPlayerScore.MostGamesOnSingleHeroId} | {players[0].PostSeasonPlayerScore.MostGamesOnSingleHero}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{players[1].PostSeasonPlayerScore.MostGamesOnSingleHeroId} | {players[0].PostSeasonPlayerScore.MostGamesOnSingleHero}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.AverageImpact).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Most Impactful Player",
            Awardee = players[0].DraftName!,
            RunnerUp = players[1].DraftName!
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderBy(x => x.PostSeasonPlayerScore.AverageImpact).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Least Impactful Player",
            Awardee = players[0].DraftName!,
            RunnerUp = players[1].DraftName!
        };
        awards.Add(award);


        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.ItemTotalBrownBoots).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Brown Boots Only Games",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.ItemTotalBrownBoots}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.ItemTotalBrownBoots}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.ItemTotalDivineRapiers).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Rapiers Owned",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.ItemTotalDivineRapiers}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.ItemTotalDivineRapiers}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.ItemTotalGemOfTrueSight).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Total Gems Owned",
            Awardee = players[0].DraftName!,
            Value = $"{players[0].PostSeasonPlayerScore.ItemTotalGemOfTrueSight}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue = $"{players[1].PostSeasonPlayerScore.ItemTotalGemOfTrueSight}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.KDAAverage).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Highest KDA Average",
            Awardee = players[0].DraftName!,
            Value =
                $"{players[0].PostSeasonPlayerScore.KDAAverage:0.00}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{players[1].PostSeasonPlayerScore.KDAAverage:0.00}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .Where(x => x.PostSeasonPlayerScore.KDAAverage > 0)
            .OrderBy(x => x.PostSeasonPlayerScore.KDAAverage).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Lowest KDA Average",
            Awardee = players[0].DraftName!,
            Value =
                $"{players[0].PostSeasonPlayerScore.KDAAverage:0.00}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{players[1].PostSeasonPlayerScore.KDAAverage:0.00}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderByDescending(x => x.PostSeasonPlayerScore.HighestKDA).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Highest KDA in a single game",
            Awardee = players[0].DraftName!,
            Value =
                $"{players[0].PostSeasonPlayerScore.HighestKDA:0.00} | {players[0].PostSeasonPlayerScore.HighestKDAHero}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{players[1].PostSeasonPlayerScore.HighestKDA:0.00} | {players[1].PostSeasonPlayerScore.HighestKDAHero}"
        };
        awards.Add(award);

        players = division.Teams.SelectMany(x => x.Players)
            .OrderBy(x => x.PostSeasonPlayerScore.LowestKDA).Where(x => x.PostSeasonPlayerScore.HighestKDAHero > 0)
            .Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Misc,
            Name = "Lowest KDA in a single game",
            Awardee = players[0].DraftName!,
            Value =
                $"{players[0].PostSeasonPlayerScore.LowestKDA:0.00} | {players[0].PostSeasonPlayerScore.LowestKDAHero}",
            RunnerUp = players[1].DraftName!,
            RunnerUpValue =
                $"{players[1].PostSeasonPlayerScore.LowestKDA:0.00} | {players[1].PostSeasonPlayerScore.LowestKDAHero}"
        };
        awards.Add(award);


        var teams = division.Teams.OrderBy(x =>
            x.Players.Max(y => y.PostSeasonPlayerScore.TotalGames) -
            x.Players.Min(y => y.PostSeasonPlayerScore.TotalGames)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Fewest Stand-in games",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Max(x => x.PostSeasonPlayerScore.TotalGames) - teams[0].Players.Min(x => x.PostSeasonPlayerScore.TotalGames)}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Max(x => x.PostSeasonPlayerScore.TotalGames) - teams[1].Players.Min(x => x.PostSeasonPlayerScore.TotalGames)}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Max(y => y.PostSeasonPlayerScore.TotalGames) -
            x.Players.Min(y => y.PostSeasonPlayerScore.TotalGames)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Stand-in games",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Max(x => x.PostSeasonPlayerScore.TotalGames) - teams[0].Players.Min(x => x.PostSeasonPlayerScore.TotalGames)}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Max(x => x.PostSeasonPlayerScore.TotalGames) - teams[1].Players.Min(x => x.PostSeasonPlayerScore.TotalGames)}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.TotalHeroesPlayed)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Heroes Played",
            Awardee = $"Team {teams[0].Name}",
            Value = $"{teams[0].Players.Sum(x => x.PostSeasonPlayerScore.TotalHeroesPlayed)}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue = $"{teams[1].Players.Sum(x => x.PostSeasonPlayerScore.TotalHeroesPlayed)}"
        };
        awards.Add(award);

        teams = division.Teams.OrderBy(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.TotalHeroesPlayed)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Fewest Heroes Played",
            Awardee = $"Team {teams[0].Name}",
            Value = $"{teams[0].Players.Sum(x => x.PostSeasonPlayerScore.TotalHeroesPlayed)}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue = $"{teams[1].Players.Sum(x => x.PostSeasonPlayerScore.TotalHeroesPlayed)}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Max(y => y.PostSeasonPlayerScore.LongestWonGameLength)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Longest Game Won (Minutes)",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{TimeSpan.FromSeconds(teams[0].Players.Max(x => x.PostSeasonPlayerScore.LongestWonGameLength)).TotalMinutes:N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{TimeSpan.FromSeconds(teams[1].Players.Max(x => x.PostSeasonPlayerScore.LongestWonGameLength)).TotalMinutes:N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderBy(x =>
            x.Players.Max(y => y.PostSeasonPlayerScore.ShortestWonGameLength)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Shortest Game Won (Minutes)",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{TimeSpan.FromSeconds(teams[0].Players.Max(x => x.PostSeasonPlayerScore.ShortestWonGameLength)).TotalMinutes:N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{TimeSpan.FromSeconds(teams[1].Players.Max(x => x.PostSeasonPlayerScore.ShortestWonGameLength)).TotalMinutes:N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderBy(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.PauseCount)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Fewest Pauses",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Sum(y => y.PostSeasonPlayerScore.PauseCount):N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Sum(y => y.PostSeasonPlayerScore.PauseCount):N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.PauseCount)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Pauses",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Sum(y => y.PostSeasonPlayerScore.PauseCount):N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Sum(y => y.PostSeasonPlayerScore.PauseCount):N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.TotalPings)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Pings",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Sum(y => y.PostSeasonPlayerScore.TotalPings):N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Sum(y => y.PostSeasonPlayerScore.TotalPings):N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.SmokeKillCount)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Smoke Kills",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Sum(y => y.PostSeasonPlayerScore.SmokeKillCount):N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Sum(y => y.PostSeasonPlayerScore.SmokeKillCount):N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.GankKillCount)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Gank Kills",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{teams[0].Players.Sum(y => y.PostSeasonPlayerScore.GankKillCount):N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{teams[1].Players.Sum(y => y.PostSeasonPlayerScore.GankKillCount):N0}"
        };
        awards.Add(award);

        teams = division.Teams.OrderByDescending(x =>
            x.Players.Sum(y => y.PostSeasonPlayerScore.TotalStunSeconds)).Take(2).ToArray();
        award = new PostSeasonAward
        {
            Category = DotaEnums.PostSeasonAwardCategory.Team,
            Name = "Most Stuns (Seconds)",
            Awardee = $"Team {teams[0].Name}",
            Value =
                $"{TimeSpan.FromMilliseconds(teams[0].Players.Sum(y => y.PostSeasonPlayerScore.TotalStunSeconds)).TotalSeconds:N0}",
            RunnerUp = $"Team {teams[1].Name}",
            RunnerUpValue =
                $"{TimeSpan.FromMilliseconds(teams[1].Players.Sum(y => y.PostSeasonPlayerScore.TotalStunSeconds)).TotalSeconds:N0}"
        };
        awards.Add(award);

        return awards;
    }
}