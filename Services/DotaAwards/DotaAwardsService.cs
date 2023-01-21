using RD2LPowerRankings.Modules.Dota;
using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Services.DotaAwards;

public class DotaAwardsService : IDotaAwardsService
{
    public List<PowerRankedPlayer> GiveDivisionPlayerAwards(List<PowerRankedPlayer> players)
    {
        int index;
        Enum.GetValues<DotaEnums.Hero>().ToList().ForEach(hero =>
        {
            index = 0;

            foreach (var player in players
                         .OrderByDescending(y =>
                             y.Heroes.FirstOrDefault(z => z.HeroId == hero && z.MatchesPlayed > 3)?.TotalScore ?? 0)
                         .Take(3))
            {
                player.Awards.Add(new PowerRankedAward(
                    $"#{index + 1} {Enum.GetName(hero)?.Replace("_", " ")} ",
                    (DotaEnums.AwardColor)(index < 3 ? index : 3)));
                index++;
            }
        });

        foreach (var player in players.Where(y =>
                     y.ToxicityScore <= DotaRankingConstants.WholesomeToxicityScoreThreshold))
        {
            player.Awards.Add(new PowerRankedAward("Wholesome Player", DotaEnums.AwardColor.Green));
        }

        index = 0;
        foreach (var player in players.OrderByDescending(y => y.Heroes.Count(z => z.MatchesPlayed > 5))
                     .Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Hero Versatility",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players.OrderBy(y => y.Heroes.Count(z => z.MatchesPlayed > 5)).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Worst Hero Versatility", DotaEnums.AwardColor.Red));
            index++;
        }

        index = 0;
        foreach (var player in players.OrderByDescending(y => y.AverageIntentionalFeeding).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Intentional Feeder", DotaEnums.AwardColor.Red));
            index++;
        }

        index = 0;
        foreach (var player in players.Where(x => x.Heroes.Any(y => y.TotalScore > 0))
                     .OrderByDescending(y => y.Heroes.Average(z => z.KDA)).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Highest Avg KDA",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players.Where(x => x.Heroes.Any(y => y.TotalScore > 0))
                     .OrderByDescending(y => y.Heroes.Where(z => z.TotalScore > 0).Average(z => z.WinRate)).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Highest win rate",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players.Where(x => x.Heroes.Any(y => y.TotalScore > 0))
                     .OrderBy(y => y.Heroes.Where(z => z.TotalScore > 0).Average(z => z.WinRate)).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Lowest win rate", DotaEnums.AwardColor.Red));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.FirstBloodAverage).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Highest 1st Blood avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.CourierKillAverage).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Courier Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageGankKills).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Gank Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageSmokeKills).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Smoke Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageInvisibleKills).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Invis Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageTpKills).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Clutch TP Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageSoloKills).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Solo Kill avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }


        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.DeniesAverage).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Deny Count Avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageTeamFightParticipation).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Fight Participation Avg",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageTowerDamage).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Objective Gamer",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageAPM).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} APM",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageStuns).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Avg StunDuration",
                (DotaEnums.AwardColor)(index < 3 ? index : 3)));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchPeruPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Peru Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchRussiaPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Russia Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchEUEastPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} EU East Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchEUWestPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} EU West Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchUSEastPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} US East Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.MatchUSWestPercent).Take(3))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} US West Match %",
                DotaEnums.AwardColor.Blue));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageLaneEfficiency).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Best Laner | Overall",
                DotaEnums.AwardColor.Green));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageLaneEfficiencySafe).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Best Laner | Safe",
                DotaEnums.AwardColor.Green));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageLaneEfficiencyOff).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Best Laner | Off",
                DotaEnums.AwardColor.Green));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageLaneEfficiencyMid).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Best Laner | Mid",
                DotaEnums.AwardColor.Green));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageSentriesPlaced).Take(5))
        {
            player.Awards.Add(new PowerRankedAward($"#{index + 1} Avg Sentries Placed",
                DotaEnums.AwardColor.Green));
            index++;
        }

        index = 0;
        foreach (var player in players
                     .OrderByDescending(y => y.AverageArmletToggles).Take(1))
        {
            player.Awards.Add(new PowerRankedAward($"Best Armlet Toggler",
                DotaEnums.AwardColor.Green));
            index++;
        }

        players[0].Awards.Add(new PowerRankedAward("1st to Sign-up", DotaEnums.AwardColor.Gold));


        foreach (var player in players)
        {
            player.Awards = player.Awards.OrderBy(x =>
                x.Color == DotaEnums.AwardColor.Gold
                    ? 1
                    : x.Color == DotaEnums.AwardColor.Silver
                        ? 2
                        : x.Color == DotaEnums.AwardColor.Bronze
                            ? 3
                            : x.Color == DotaEnums.AwardColor.Green
                                ? 0
                                : x.Color == DotaEnums.AwardColor.Blue
                                    ? 5
                                    : 6).ToList();
        }


        return players;
    }

    public PowerRankedDivision GiveDivisionTeamAwards(PowerRankedDivision division)
    {
        foreach (var team in division.Teams)
        {
            if (team.Players.Count(x => x.Loccountrycode == "US") >= 5)
            {
                team.Awards.Add(new PowerRankedAward("All-American Team", DotaEnums.AwardColor.Blue));
            }

            if (team.Players.Count(x => x.Loccountrycode == "CA") >= 5)
            {
                team.Awards.Add(new PowerRankedAward("All-Canadian Team", DotaEnums.AwardColor.Blue));
            }
        }

        division.Teams.OrderByDescending(x => x.Players.Sum(y => y.DeniesAverage)).First().Awards
            .Add(new PowerRankedAward("Best Deny Average", DotaEnums.AwardColor.Green));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.ToxicityScore)).First().Awards
            .Add(new PowerRankedAward("Most Wholesome", DotaEnums.AwardColor.Green));

        division.Teams.OrderByDescending(x => x.Players.Sum(y => y.ToxicityScore)).First().Awards
            .Add(new PowerRankedAward("Most Toxic", DotaEnums.AwardColor.Red));

        division.Teams.OrderByDescending(x => x.Players
                .Where(y => y.TeamRole is DotaEnums.TeamRole.HardSupport or DotaEnums.TeamRole.SoftSupport)
                .Sum(y => y.SoftSupportScore + y.HardSupportScore)).First()
            .Awards.Add(new PowerRankedAward("Best Support Duo", DotaEnums.AwardColor.Green));

        division.Teams.OrderByDescending(x => x.Players
                .Where(y => y.TeamRole is DotaEnums.TeamRole.Midlane or DotaEnums.TeamRole.Safelane
                    or DotaEnums.TeamRole.Offlane)
                .Sum(y => y.SafelaneScore + y.OfflaneScore + y.MidlaneScore))
            .First().Awards.Add(new PowerRankedAward("Best Core Trio", DotaEnums.AwardColor.Green));

        division.Teams.OrderByDescending(x => x.Players
                .Where(y => y.TeamRole is DotaEnums.TeamRole.Midlane or DotaEnums.TeamRole.Safelane
                    or DotaEnums.TeamRole.Offlane)
                .Sum(y => Math.Max(y.SafelaneScore + y.MidlaneScore,
                    Math.Max(y.SafelaneScore + y.OfflaneScore, y.MidlaneScore + y.OfflaneScore))))
            .First().Awards.Add(new PowerRankedAward("Best Core Duo", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.Heroes.Where(z => z.MatchesPlayed > 5).ToList().Count)).First()
            .Awards
            .Add(new PowerRankedAward("Best Hero Versatility", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.Heroes.Sum(z => z.MatchesPlayed * z.LeagueMatchMakingPercent)))
            .First()
            .Awards
            .Add(new PowerRankedAward("RD2L Vets", DotaEnums.AwardColor.Blue));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AveragePauses)).First().Awards
            .Add(new PowerRankedAward("Bathroom Gamers", DotaEnums.AwardColor.Blue));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageRandomHeroes)).First().Awards
            .Add(new PowerRankedAward("Random Hero Gamers", DotaEnums.AwardColor.Blue));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageScans)).First().Awards
            .Add(new PowerRankedAward("Scanning Enthusiasts", DotaEnums.AwardColor.Green));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageInvisibleKills + y.AverageSoloKills)).First().Awards
            .Add(new PowerRankedAward("Stealth Assassins", DotaEnums.AwardColor.Green));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageTpKills)).First().Awards
            .Add(new PowerRankedAward("Clutch Killers", DotaEnums.AwardColor.Green));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageTpKills)).First().Awards
            .Add(new PowerRankedAward("Run Down Mid Gamers", DotaEnums.AwardColor.Red));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageGankKills)).First().Awards
            .Add(new PowerRankedAward("Roaming Enthusiasts", DotaEnums.AwardColor.Green));

        division.Teams.OrderBy(x => x.Players.Sum(y => y.AverageSmokeKills)).First().Awards
            .Add(new PowerRankedAward("Smoke Specialists", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.Heroes.Sum(z => z.MatchesPlayed * z.LeagueMatchMakingPercent))).First()
            .Awards
            .Add(new PowerRankedAward("RD2L Noobs", DotaEnums.AwardColor.Blue));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.Heroes.Where(z => z.MatchesPlayed > 5).ToList().Count)).First()
            .Awards
            .Add(new PowerRankedAward("Most Hero Spammers", DotaEnums.AwardColor.Blue));

        division.Teams
            .OrderByDescending(x => x.Players.Average(y => y.AverageExcessPings)).First()
            .Awards
            .Add(new PowerRankedAward("Ping Enthusiasts", DotaEnums.AwardColor.Blue));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.CourierKillAverage)).First()
            .Awards
            .Add(new PowerRankedAward("Courier Slayers", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.FirstBloodAverage)).First()
            .Awards
            .Add(new PowerRankedAward("First Blood Gamers", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.CampsStackedAverage)).First()
            .Awards
            .Add(new PowerRankedAward("Best Creep Stackers", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.AverageAPM)).First()
            .Awards
            .Add(new PowerRankedAward("Highest APM Team", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.AverageAPM)).First()
            .Awards
            .Add(new PowerRankedAward("Lowest APM Team", DotaEnums.AwardColor.Red));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.AverageTeamFightParticipation)).First()
            .Awards
            .Add(new PowerRankedAward("Most Fight Participation", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.AverageTeamFightParticipation)).First()
            .Awards
            .Add(new PowerRankedAward("Least Fight Participation", DotaEnums.AwardColor.Red));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.AverageStuns)).First()
            .Awards
            .Add(new PowerRankedAward("Most Stunning Team", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.AverageTowerDamage)).First()
            .Awards
            .Add(new PowerRankedAward("Objective Gamers", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.AverageLaneEfficiency)).First()
            .Awards
            .Add(new PowerRankedAward("Best Laners", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.Heroes.Average(y => y.KDA))).First()
            .Awards
            .Add(new PowerRankedAward("Highest Avg KDA", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.Heroes.Average(y => y.KDA))).First()
            .Awards
            .Add(new PowerRankedAward("Lowest Avg KDA", DotaEnums.AwardColor.Red));

        division.Teams
            .OrderByDescending(x => x.Players.Sum(y => y.Heroes.Average(y => y.WinRate))).First()
            .Awards
            .Add(new PowerRankedAward("Highest Avg Winrate", DotaEnums.AwardColor.Green));

        division.Teams
            .OrderBy(x => x.Players.Sum(y => y.Heroes.Average(y => y.WinRate))).First()
            .Awards
            .Add(new PowerRankedAward("Lowest Avg Winrate", DotaEnums.AwardColor.Red));

        return division;
    }
}