using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Services.DotaRanking.Enums;
using RD2LPowerRankings.Services.PlayerDataSource;
using RestSharp;

namespace RD2LPowerRankings.Services.DotaDataSource;

public class StratzDotaDataSource : IDotaDataSource
{
    private readonly IConfiguration _config;
    private readonly DotaDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<StratzDotaDataSource> _logger;
    private readonly IPlayerDataSource _playerDataSource;
    private readonly RestClient _restClient;

    public StratzDotaDataSource(
        ILogger<StratzDotaDataSource> logger,
        IConfiguration config,
        IPlayerDataSource playerDataSource,
        DotaDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _playerDataSource = playerDataSource;
        _context = context;
        _config = config;
        _httpClient = httpClientFactory.CreateClient(nameof(StratzDotaDataSource));
        _httpClient.Timeout = TimeSpan.FromMinutes(60);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config["Stratz:ApiKey"]);
        _httpClient.BaseAddress = new Uri(_config["Stratz:ApiUrl"] ?? "");
        _restClient = new RestClient("https://api.stratz.com/graphql");
    }

    public async Task LoadPlayerData(string sheetId)
    {
        var players = _playerDataSource.GetPlayers(sheetId);

        foreach (var player in players)
        {
            await SavePlayer(player.Id, player.Name);

            await SaveMatches(player.Id);
        }
    }

    private async Task SavePlayer(long playerId, string draftName)
    {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == playerId) ??
                     new Player { PlayerId = playerId };

        // Don't refresh player data if we have done so recently
        if ((DateTime.Now - player.UpdatedAt).Days <= DotaDataConstants.CacheDays)
        {
            return;
        }

        var response = await
            _httpClient.GetAsync($"/api/v1/Player/{playerId}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        dynamic playerRaw = JsonConvert.DeserializeObject(content)!;

        player.Avatar = playerRaw["steamAccount"]["avatar"];
        player.Loccountrycode = player.Loccountrycode;
        player.PersonaName = playerRaw["steamAccount"]["name"];
        player.DraftName = draftName;
        player.Profileurl = playerRaw["steamAccount"]["profileUri"];
        player.LastLogin = playerRaw["steamAccount"]["lastActiveTime"];
        player.Steamid = playerRaw["steamAccount"]["id"];
        player.SoloRank = playerRaw["steamAccount"]["soloRank"];
        player.PartyRank = playerRaw["steamAccount"]["partyRank"];
        player.SmurfFlag = playerRaw["steamAccount"]["smurfFlag"];
        player.BehaviorScore = playerRaw["behaviorScore"];

        player.CreatedAt ??= DateTime.Now.ToUniversalTime();
        player.UpdatedAt = DateTime.Now.ToUniversalTime();

        await _context.Players.Upsert(player).On(x => x.PlayerId).RunAsync();
    }

    private async Task SaveMatches(long playerId)
    {
        var skip = 0;
        var take = 100;
        var maxGames = 2000;
        int? lastMatchParseTime = null;

        var player = await _context.Players.FirstAsync(x => x.PlayerId == playerId);

        // we're up to date on parsing this players matches.
        if (player.LastMatchParseDate >= (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch)
                .Subtract(new TimeSpan(3, 0, 0, 0)).TotalSeconds)
        {
            return;
        }

        while (skip <= maxGames)
        {
            var startTime = player.LastMatchParseDate ?? (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch)
                .Subtract(new TimeSpan(DotaDataConstants.MatchRetrievalDays, 0, 0, 0)).TotalSeconds;

            var content = await QueryMatchDetailsByGraphQl(playerId, take, skip,
                startTime,
                (long)DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds);

            dynamic matchesRaw = JsonConvert.DeserializeObject(content)!;

            var matchCount = matchesRaw["data"]["player"]["matchesMatches"].Count;
            // If we didn't reach the max games to parse for player, end parsing.
            if (matchCount == 0)
            {
                break;
            }

            // this is the last set of matches, no need to make an extra API call just to get zero results.
            if (matchCount < 100)
            {
                skip = 42069; // sometimes I like magic numbers 🤡
            }

            foreach (JObject matchRaw in matchesRaw["data"]["player"]["matchesMatches"])
            {
                var match = new Match();

                match.MatchId = matchRaw["id"]?.Value<long?>() ?? -1L;
                match.Duration = matchRaw["durationSeconds"]?.Value<int?>();

                match.GameMode = DotaDataHelpers.GameModeTextToEnum(matchRaw["gameMode"]?.Value<string>() ?? "");
                match.LeagueId = matchRaw["leagueId"]?.Value<int?>();
                match.LobbyType = DotaDataHelpers.LobbyTypeTextToEnum(matchRaw["lobbyType"]?.Value<string?>() ?? "");
                match.StartTime = matchRaw["endDateTime"]?.Value<int?>();
                match.Rank = matchRaw["rank"]?.Value<int?>();
                match.Region = (DotaEnums.Region)(matchRaw["regionId"]?.Value<int?>() ?? -1);

                // ignore unscored or corrupted games.
                if (!matchRaw["radiantKills"].HasValues && !matchRaw["direKills"].HasValues)
                {
                    continue;
                }

                foreach (var kill in matchRaw["radiantKills"])
                {
                    match.RadiantKills += kill.Value<int>();
                }

                foreach (var kill in matchRaw["direKills"])
                {
                    match.DireKills += kill.Value<int>();
                }

                match.Loaded = false;


                // Ensure we don't include that have been loaded or are not relevant.
                if ((await FilterMatches(new List<Match> { match })).Count == 0)
                {
                    continue;
                }

                await GetMatchDetails(match, matchRaw);

                if (lastMatchParseTime == null || match.StartTime > lastMatchParseTime)
                {
                    lastMatchParseTime = match.StartTime ?? 0;
                }
            }


            skip += take;
        }

        // prevent re-fetching matches that have already been parsed.
        player.LastMatchParseDate = lastMatchParseTime;
        await _context.Players.Upsert(player).On(x => x.PlayerId).RunAsync();
    }

    private async Task<List<Match>> FilterMatches(List<Match> matches)
    {
        // filter only relevant, parsed matches.
        matches = matches.Where(x => x.LobbyType
                                         is DotaEnums.LobbyType.BattleCup
                                         or DotaEnums.LobbyType.Ranked
                                         or DotaEnums.LobbyType.Normal
                                     || x is
                                     {
                                         LobbyType: DotaEnums.LobbyType.Practice,
                                         GameMode: DotaEnums.GameMode.CaptainsMode
                                     })
            .ToList();

        return matches;
    }

    private async Task GetMatchDetails(Match match, dynamic matchDetails)
    {
        try
        {
            match.CreatedAt ??= DateTime.Now.ToUniversalTime();


            match.UpdatedAt = DateTime.Now.ToUniversalTime();
            await _context.Matches.Upsert(match).On(x => x.MatchId).RunAsync();


            var firstBloodTime = 0;
            var firstBloodPlayerId = 0L;

            if (!matchDetails["allPlayers"][0]["stats"]["actionsPerMinute"].HasValues)
            {
                return;
            }

            foreach (JObject player in matchDetails["allPlayers"])
            {
                // Ignore anonymous users.
                if (player["steamAccountId"]?.Value<long?>() == null)
                {
                    continue;
                }

                var playerMatch = new PlayerMatch();
                playerMatch.LeagueId = match.LeagueId ?? -1;
                playerMatch.MatchId = match.MatchId;
                playerMatch.MatchRank = match.Rank;
                playerMatch.PlayerId = player["steamAccountId"]?.Value<long?>() ?? 0;
                playerMatch.CreatedAt ??= DateTime.Now.ToUniversalTime();
                playerMatch.UpdatedAt = DateTime.Now.ToUniversalTime();
                playerMatch.Assists = player["assists"]?.Value<int?>() ?? 0;
                playerMatch.Award = player["award"]?.Value<string?>() ?? "";
                playerMatch.Backpack0 = player["backpack0Id"]?.Value<int?>();
                playerMatch.Backpack1 = player["backpack1Id"]?.Value<int?>();
                playerMatch.Backpack2 = player["backpack2Id"]?.Value<int?>();
                playerMatch.Backpack3 = null;

                foreach (var stack in player["stats"]?["campStack"])
                {
                    if (stack.Value<int?>() > 0 && stack.Value<int>() > playerMatch.CampsStacked)
                    {
                        playerMatch.CampsStacked = stack.Value<int>();
                    }
                }


                playerMatch.Deaths = player["deaths"]?.Value<int?>() ?? 0;
                playerMatch.Denies = player["numDenies"]?.Value<int?>() ?? 0;
                playerMatch.FirstbloodClaimed = false;
                playerMatch.Gold = player["gold"]?.Value<int?>() ?? 0;
                playerMatch.GoldSpent = player["goldSpent"]?.Value<int?>() ?? 0;
                playerMatch.GoldPerMin = player["goldPerMinute"]?.Value<int?>() ?? 0;
                playerMatch.HeroDamage = player["heroDamage"]?.Value<long?>() ?? 0;
                playerMatch.HeroHealing = player["heroHealing"]?.Value<long?>() ?? 0;
                playerMatch.HeroId = (DotaEnums.Hero)(player["heroId"]?.Value<int?>() ?? 0);
                playerMatch.Imp = player["imp"]?.Value<int?>() ?? 0;
                playerMatch.IntentionalFeeding = player["intentionalFeeding"]?.Value<bool?>() ?? false;
                playerMatch.IsRadiant = player["isRadiant"]?.Value<bool?>() ?? false;
                playerMatch.Item0 = (DotaEnums.Item?)player["item0Id"]?.Value<long?>();
                playerMatch.Item1 = (DotaEnums.Item?)player["item1Id"]?.Value<long?>();
                playerMatch.Item2 = (DotaEnums.Item?)player["item2Id"]?.Value<long?>();
                playerMatch.Item3 = (DotaEnums.Item?)player["item3Id"]?.Value<long?>();
                playerMatch.Item4 = (DotaEnums.Item?)player["item4Id"]?.Value<long?>();
                playerMatch.Item5 = (DotaEnums.Item?)player["item5Id"]?.Value<long?>();
                playerMatch.ItemNeutral = (DotaEnums.Item?)player["neutral0Id"]?.Value<long?>();
                playerMatch.Kills = player["kills"]?.Value<int?>() ?? 0;
                playerMatch.LastHits = player["numLastHits"]?.Value<int?>() ?? 0;
                playerMatch.LeaverStatus = player["leaverStatus"]?.Value<string>() ?? "NONE";
                playerMatch.Level = player["level"]?.Value<int?>() ?? 0;
                playerMatch.NetWorth = player["networth"]?.Value<long?>() ?? 0;

                var obsCount = 0;
                var senCount = 0;
                foreach (JObject ward in player["stats"]?["wards"])
                {
                    if (ward["type"]?.Value<int>() == 0)
                    {
                        obsCount++;
                    }
                    else
                    {
                        senCount++;
                    }
                }

                playerMatch.ObsPlaced = obsCount;
                playerMatch.SenPlaced = senCount;
                playerMatch.PartyId = player["partyId"]?.Value<int?>();
                if (player["stats"]?["actionReport"]?.HasValues ?? false)
                {
                    playerMatch.Pings = player["stats"]?["actionReport"]?["pingUsed"]?.Value<int?>() ?? 0;
                    playerMatch.Scans = player["stats"]?["actionReport"]?["scanUsed"]?.Value<int?>() ?? 0;
                }

                playerMatch.Randomed = player["isRandom"]?.Value<bool?>() ?? false;
                playerMatch.RunePickups = player["stats"]?["runes"]?.Count() ?? 0;

                if ((playerMatch.IsRadiant && match.RadiantKills > 0) ||
                    (!playerMatch.IsRadiant && match.DireKills > 0))
                {
                    playerMatch.TeamfightParticipation = (playerMatch.Kills + playerMatch.Assists) * 1M /
                        (playerMatch.IsRadiant ? match.RadiantKills : match.DireKills) * 1M;
                }


                playerMatch.TowerDamage = player["towerDamage"]?.Value<long?>() ?? 0;
                playerMatch.XpPerMin = player["experiencePerMinute"]?.Value<int?>() ?? 0;
                playerMatch.StartTime = match.StartTime ?? 0;
                playerMatch.Duration = match.Duration ?? 0;
                playerMatch.LobbyType = match.LobbyType;
                playerMatch.Region = match.Region;
                playerMatch.Win = player["isVictory"]?.Value<bool?>() ?? false;
                playerMatch.Lose = !(player["isVictory"]?.Value<bool?>() ?? false);
                playerMatch.TotalGold = player["gold"]?.Value<long?>() ?? 0;
                playerMatch.Kda = (playerMatch.Kills + playerMatch.Assists) /
                                  (playerMatch.Deaths == 0 ? 1M : playerMatch.Deaths * 1M);
                playerMatch.Abandons = player["leaverStatus"]?.Value<string?>() == "NONE" ? 0 : 1;
                playerMatch.CourierKills = player["stats"]?["courierKills"]?.Count() ?? 0;

                if (player["stats"]?["networthPerMinute"]?.Count() > 10)
                {
                    playerMatch.LaneEfficiencyPct =
                        (int)(player["stats"]?["networthPerMinute"]?[10]?.Value<int?>() ?? 1M * 1M / 4000 * 100M);
                }

                playerMatch.Lane = DotaDataHelpers.LaneStringToEnum(player["lane"]?.Value<string>()!);

                playerMatch.LaneRole =
                    DotaDataHelpers.RoleStringToEnum(player["role"].Value<string>(), playerMatch.Lane);
                var totalActions = 0;

                if (player["stats"]["actionsPerMinute"].HasValues)
                {
                    foreach (var action in player["stats"]?["actionsPerMinute"])
                    {
                        totalActions += action.Value<int>();
                    }

                    playerMatch.ActionsPerMinute = totalActions / player["stats"]["actionsPerMinute"].Count();
                }

                if (player["stats"]["allTalks"].HasValues)
                {
                    playerMatch.PauseCount = player["stats"]["allTalks"].Count();
                }

                var timeDead = 0;
                var goldFed = 0;
                var deathTpCount = 0;
                var goldLost = 0;


                foreach (var action in player["stats"]["deathEvents"])
                {
                    timeDead += action["timeDead"]?.Value<int?>() ?? 0;
                    goldFed = action["goldFed"]?.Value<int?>() ?? 0;
                    goldLost = action["goldLost"]?.Value<int?>() ?? 0;
                    deathTpCount += action["isAttemptTpOut"]?.Value<bool?>() ?? false ? 1 : 0;
                }

                var smokeKillCount = 0;
                var soloKillCount = 0;
                var invisibleKillCount = 0;
                var tpRecentlyKillCount = 0;
                var gankKillCount = 0;

                foreach (var action in player["stats"]["killEvents"])
                {
                    smokeKillCount += action["isSmoke"]?.Value<bool?>() ?? false ? 1 : 0;
                    soloKillCount += action["isSolo"]?.Value<bool?>() ?? false ? 1 : 0;
                    invisibleKillCount += action["isInvisible"]?.Value<bool?>() ?? false ? 1 : 0;
                    tpRecentlyKillCount += action["isTpRecently"]?.Value<bool?>() ?? false ? 1 : 0;
                    gankKillCount += action["isGank"]?.Value<bool?>() ?? false ? 1 : 0;

                    if (firstBloodTime == 0 || firstBloodTime > action["time"]?.Value<int>())
                    {
                        firstBloodTime = action["time"]?.Value<int?>() ?? 0;
                        firstBloodPlayerId = playerMatch.PlayerId;
                    }
                }

                playerMatch.SmokeKillCount = smokeKillCount;
                playerMatch.SoloKillCount = soloKillCount;
                playerMatch.InvisibleKillCount = invisibleKillCount;
                playerMatch.TpRecentlyKillCount = tpRecentlyKillCount;
                playerMatch.GankKillCount = gankKillCount;


                playerMatch.LifeStateDead = timeDead;
                playerMatch.DeathTpAttemptCount = deathTpCount;
                playerMatch.GoldFed = goldFed;
                playerMatch.GoldLost = goldLost;

                playerMatch.StunDuration =
                    player["stats"]?["heroDamageReport"]?["dealtTotal"]?["stunDuration"]?.Value<long?>() ?? 0L;
                playerMatch.SlowDuration =
                    player["stats"]?["heroDamageReport"]?["dealtTotal"]?["slowDuration"]?.Value<long?>() ?? 0L;

                await _context.PlayerMatches.Upsert(playerMatch).On(x => new { x.PlayerId, x.MatchId }).RunAsync();

                var abilities = new List<PlayerMatchAbility>();
                foreach (var itemRaw in player["stats"]?["abilityCastReport"]!)
                {
                    var item = new PlayerMatchAbility();
                    item.Count = itemRaw["count"]?.Value<long?>() ?? 0;
                    item.AbilityId = itemRaw["abilityId"]?.Value<long?>() ?? -1L;
                    item.MatchId = match.MatchId;
                    item.PlayerId = playerMatch.PlayerId;
                    abilities.Add(item);
                }

                await _context.PlayerMatchAbilities.UpsertRange(abilities)
                    .On(x => new { x.PlayerId, x.MatchId, x.AbilityId }).RunAsync();


                var itemUses = new List<PlayerMatchItemUse>();
                foreach (var itemRaw in player["stats"]?["itemUsed"]!)
                {
                    var item = new PlayerMatchItemUse();
                    item.ItemId = itemRaw["itemId"]?.Value<long?>() ?? -1;
                    item.Uses = itemRaw["count"]?.Value<long?>() ?? 0;
                    item.MatchId = match.MatchId;
                    item.PlayerId = playerMatch.PlayerId;
                    itemUses.Add(item);
                }

                await _context.PlayerMatchItemUses.UpsertRange(itemUses)
                    .On(x => new { x.PlayerId, x.MatchId, x.ItemId }).RunAsync();
            }

            foreach (JObject player in matchDetails["allPlayers"])
            {
                // Ignore anonymous users.
                if (player["steamAccountId"]?.Value<long?>() == null)
                {
                    continue;
                }


                foreach (var action in player["stats"]["killEvents"])
                {
                    var playerKill = new PlayerMatchKill();

                    var targetHeroId = action["target"]?.Value<int>() ?? -1;

                    if (targetHeroId < 0)
                    {
                        continue;
                    }


                    playerKill.PlayerId = player["steamAccountId"]?.Value<long?>() ?? 0;
                    playerKill.PlayerHeroId = (DotaEnums.Hero)(player["heroId"]?.Value<int?>() ?? 0);
                    playerKill.TargetId = _context.PlayerMatches.First(x =>
                        x.MatchId == match.MatchId && x.HeroId == (DotaEnums.Hero)targetHeroId).PlayerId;
                    playerKill.TargetHeroId = (DotaEnums.Hero)targetHeroId;
                    playerKill.Time = action["time"]?.Value<long?>();
                    playerKill.MatchId = match.MatchId;
                    playerKill.ItemId = action["byItem"]?.Value<long?>();
                    playerKill.AbilityId = action["byAbility"]?.Value<long?>();

                    await _context.PlayerMatchKills.Upsert(playerKill)
                        .On(x => new { x.PlayerId, x.MatchId, x.TargetId, x.Time }).RunAsync();
                }
            }

            var fbPlayer =
                await _context.PlayerMatches.FirstOrDefaultAsync(x =>
                    x.PlayerId == firstBloodPlayerId && x.MatchId == match.MatchId);

            if (fbPlayer != null)
            {
                fbPlayer.FirstbloodClaimed = true;
                await _context.PlayerMatches.Upsert(fbPlayer).On(x => new { x.PlayerId, x.MatchId }).RunAsync();
            }


            match.Loaded = true;
            await _context.Matches.Upsert(match).On(x => x.MatchId).RunAsync();
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Could not fetch match with id {MatchId}. Adding to unparsed match list", match.MatchId);

            await _context.UnParsedMatches.Upsert(new UnParsedMatch
                {
                    MatchId = match.MatchId, FailureReason = DotaDataEnums.MatchParseFailureReason.InternalServerError,
                    ParseRequestSent = false
                }).On(x => x.MatchId)
                .RunAsync();
        }
    }


    private async Task<string?> QueryMatchDetailsByGraphQl(long playerId, int take, int skip, long startTime,
        long endTime)
    {
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Authorization",
            $"Bearer {_config["Stratz:ApiKey"]}");
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json",
            "{\"query\":\"query GetPlayerOverview($steamId: Long!, $matchesMatchesRequest: PlayerMatchesRequestType!, $activityMatchesGroupByRequest: PlayerMatchesGroupByRequestType!) {\\r\\n  player(steamAccountId: $steamId) {\\r\\n    steamAccountId\\r\\n    ...PlayerOverviewMatchesPlayerTypeFragment\\r\\n    ...PlayerOverviewActivityPlayerTypeFragment\\r\\n    __typename\\r\\n  }\\r\\n}\\r\\n\\r\\nfragment PlayerOverviewMatchesPlayerTypeFragment on PlayerType {\\r\\n  steamAccountId\\r\\n  matchesMatches: matches(request: $matchesMatchesRequest) {\\r\\n    ...MatchRowOverview\\r\\n    players(steamAccountId: $steamId) {\\r\\n      ...MatchRowOverviewPlayer\\r\\n      __typename\\r\\n    }\\r\\n    __typename\\r\\n  }\\r\\n  __typename\\r\\n}\\r\\n\\r\\nfragment MatchRowBase on MatchType {\\r\\n  id\\r\\n  lobbyType\\r\\n  gameMode\\r\\n  endDateTime\\r\\n  durationSeconds\\r\\n  averageRank\\r\\n  rank\\r\\n  leagueId\\r\\n  regionId\\r\\n  radiantKills\\r\\n  direKills\\r\\n  allPlayers: players {\\r\\n    matchId\\r\\n    playerSlot\\r\\n    isRadiant\\r\\n    heroId\\r\\n    steamAccountId\\r\\n    isRadiant\\r\\n    kills\\r\\n    deaths\\r\\n    assists\\r\\n    leaverStatus\\r\\n    numLastHits\\r\\n    numDenies\\r\\n    goldPerMinute\\r\\n    experiencePerMinute\\r\\n    level\\r\\n    gold\\r\\n    goldSpent\\r\\n    heroDamage\\r\\n    towerDamage\\r\\n    partyId\\r\\n    isRandom\\r\\n    lane\\r\\n    intentionalFeeding\\r\\n    role\\r\\n    imp\\r\\n    award\\r\\n    item0Id\\r\\n    item1Id\\r\\n    item2Id\\r\\n    item3Id\\r\\n    item4Id\\r\\n    item5Id\\r\\n    backpack0Id\\r\\n    backpack1Id\\r\\n    backpack2Id\\r\\n    heroHealing\\r\\n    lane\\r\\n    isVictory\\r\\n    networth\\r\\n    neutral0Id\\r\\n    dotaPlusHeroXp\\r\\n    invisibleSeconds\\r\\n    streakPrediction\\r\\n    stats {\\r\\n        tripsFountainPerMinute \\r\\n        assistEvents {\\r\\n            time\\r\\n        }\\r\\n        abilityCastReport {\\r\\n            abilityId\\r\\n            count\\r\\n        }\\r\\n        allTalks {\\r\\n            pausedTick\\r\\n        }\\r\\n        deathEvents {\\r\\n            timeDead\\r\\n            isAttemptTpOut\\r\\n            isDieBack\\r\\n            isBurst\\r\\n            byAbility\\r\\n            byItem\\r\\n            goldFed\\r\\n            goldLost\\r\\n        }\\r\\n        runes {\\r\\n            rune\\r\\n        }\\r\\n        actionReport{\\r\\n            pingUsed\\r\\n            scanUsed\\r\\n\\r\\n        }\\r\\n        heroDamageReport {\\r\\n                dealtTotal {\\r\\n                    stunCount\\r\\n                    stunDuration\\r\\n                    slowCount\\r\\n                    slowDuration\\r\\n                }\\r\\n        }\\r\\n        itemUsed {\\r\\n            count\\r\\n            itemId\\r\\n        }\\r\\n        itemPurchases {\\r\\n            itemId\\r\\n            time\\r\\n        }\\r\\n        campStack,\\r\\n        actionsPerMinute,\\r\\n        deniesPerMinute\\r\\n        networthPerMinute\\r\\n        wards {\\r\\n            type\\r\\n        }\\r\\n        courierKills {\\r\\n            time\\r\\n        }\\r\\n        killEvents {\\r\\n            isSmoke\\r\\n            isSolo\\r\\n            isInvisible\\r\\n            assist\\r\\n            isGank\\r\\n            byAbility\\r\\n            byItem\\r\\n            time\\r\\n           isTpRecently\\r\\n           target\\r\\n        }\\r\\n    }\\r\\n    __typename\\r\\n  }\\r\\n  analysisOutcome\\r\\n  __typename\\r\\n}\\r\\n\\r\\nfragment MatchRowBasePlayer on MatchPlayerType {\\r\\n  steamAccountId\\r\\n  heroId\\r\\n  role\\r\\n  lane\\r\\n  level\\r\\n  isVictory\\r\\n  isRadiant\\r\\n  additionalUnit {\\r\\n      item0Id\\r\\n  }\\r\\n  partyId\\r\\n  __typename\\r\\n}\\r\\n\\r\\nfragment MatchRowOverview on MatchType {\\r\\n  ...MatchRowBase\\r\\n  bottomLaneOutcome\\r\\n  midLaneOutcome\\r\\n  topLaneOutcome\\r\\n  __typename\\r\\n}\\r\\n\\r\\nfragment MatchRowOverviewPlayer on MatchPlayerType {\\r\\n  ...MatchRowBasePlayer\\r\\n  imp\\r\\n  award\\r\\n  kills\\r\\n  deaths\\r\\n  assists\\r\\n  __typename\\r\\n}\\r\\n\\r\\n\\r\\n\\r\\n\\r\\n\\r\\nfragment PlayerOverviewActivityPlayerTypeFragment on PlayerType {\\r\\n  activity {\\r\\n    activity\\r\\n    __typename\\r\\n  }\\r\\n  activityMatchesGroupBy: matchesGroupBy(request: $activityMatchesGroupByRequest) {\\r\\n    __typename\\r\\n  }\\r\\n  __typename\\r\\n}\"," +
            "\"variables\":{\"matchMetersMatchesGroupByRequest\":{\"take\":0,\"groupBy\":\"IS_VICTORY\",\"playerList\":\"SINGLE\"},\"matchMetersSkipMatchesGroupBy\":true,\"steamId\":" +
            playerId + "," +
            "\"matchesMatchesRequest\":{ \"isParsed\":true, \"take\":" + take + ",\"skip\":" + skip +
            ",\"startDateTime\":" + startTime +
            ",\"endDateTime\":" + endTime +
            ",\"gameModeIds\":[1,2,3,4,16,22],\"lobbyTypeIds\":[0,1,7,9]},\"trendsMatchesRequest\":{\"take\":0},\"activityMatchesGroupByRequest\":{\"take\":0,\"groupBy\":\"DATE_DAY\",\"playerList\":\"SINGLE\"}}}",
            ParameterType.RequestBody);
        var response = await _restClient.ExecuteAsync(request);

        return response.Content;
    }
}