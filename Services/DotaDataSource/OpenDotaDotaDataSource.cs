using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Services.DotaRanking.Enums;
using RD2LPowerRankings.Services.PlayerDataSource;

namespace RD2LPowerRankings.Services.DotaDataSource;

public class OpenDotaDotaDataSource : IDotaDataSource
{
    private readonly IConfiguration _config;
    private readonly DotaDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenDotaDotaDataSource> _logger;
    private readonly IPlayerDataSource _playerDataSource;

    public OpenDotaDotaDataSource(
        ILogger<OpenDotaDotaDataSource> logger,
        IConfiguration config,
        IPlayerDataSource playerDataSource,
        DotaDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _playerDataSource = playerDataSource;
        _context = context;
        _config = config;
        _httpClient = httpClientFactory.CreateClient(nameof(OpenDotaDotaDataSource));
        _httpClient.Timeout = TimeSpan.FromMinutes(60);
        _httpClient.BaseAddress = new Uri(_config["OpenDota:ApiUrl"]);
    }


    public async Task LoadPlayerData(string sheetId)
    {
        var players = _playerDataSource.GetPlayers(sheetId);

        foreach (var player in players)
        {
            await SavePlayer(player.Id, player.Name);

            await SaveMatches(player.Id);

            await SaveWords(player.Id);
            
            await ParseReplays();
        }
    }

    private async Task SaveWords(long playerId)
    {
        var response = await
            _httpClient.GetAsync($"/api/players/{playerId}/wordcloud?api_key={_config["OpenDota:ApiKey"]}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        dynamic playerWordsRaw = JsonConvert.DeserializeObject(content)!;

        Dictionary<string, int> playerWords = playerWordsRaw["my_word_counts"].ToObject<Dictionary<string, int>>();


        var words = new List<PlayerWord>();
        foreach (var playerWord in playerWords)
        {
            words.Add(new PlayerWord
            {
                Count = playerWord.Value, Word = playerWord.Key, PlayerId = playerId,
                UpdatedAt = DateTime.Now.ToUniversalTime()
            });
        }

        await _context.PlayerWords.UpsertRange(words).On(x => new { x.PlayerId, x.Word }).RunAsync();


        _logger.LogInformation("Added {Words} word(s) for player {Player}", words.Count, playerId);
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
            _httpClient.GetAsync($"/api/players/{playerId}/?api_key={_config["OpenDota:ApiKey"]}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        dynamic playerRaw = JsonConvert.DeserializeObject(content)!;

        player.Avatar = playerRaw["profile"]["avatarfull"];
        player.Cheese = playerRaw["profile"]["cheese"];
        player.Loccountrycode = playerRaw["profile"]["loccountrycode"];
        player.PersonaName = playerRaw["profile"]["personaname"];
        player.DraftName = draftName;
        player.Profileurl = playerRaw["profile"]["profileurl"];
        player.LastLogin = playerRaw["profile"]["last_login"];
        player.Steamid = playerRaw["profile"]["steamid"];
        player.IsContributor = playerRaw["profile"]["is_contributor"];
        player.RankTier = playerRaw["rank_tier"];
        player.MmrEstimate = playerRaw["mmr_estimate"]["estimate"];
        player.LeaderboardRank = playerRaw["leaderboard_rank"];
        player.CreatedAt ??= DateTime.Now.ToUniversalTime();
        player.UpdatedAt = DateTime.Now.ToUniversalTime();

        await _context.Players.Upsert(player).On(x => x.PlayerId).RunAsync();
    }

    private async Task SaveMatches(long playerId)
    {
        var response = await
            _httpClient.GetAsync(
                $"/api/players/{playerId}/matches/" +
                $"?api_key={_config["OpenDota:ApiKey"]}" +
                $"&date={DotaDataConstants.MatchRetrievalDays}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var matches = new List<Match>();
        dynamic matchesRaw = JsonConvert.DeserializeObject(content)!;
        foreach (var matchRaw in matchesRaw)
        {
            var match = new Match();
            match.MatchId = matchRaw["match_id"];
            match.RadiantWin = matchRaw["radiant_win"];
            match.Duration = matchRaw["duration"];
            match.GameMode = matchRaw["game_mode"];
            match.LobbyType = matchRaw["lobby_type"];
            match.StartTime = matchRaw["start_time"];
            match.Version = matchRaw["version"];
            match.Skill = matchRaw["skill"];
            match.Loaded = false;
            matches.Add(match);
        }

        // Ensure we don't fetch matches that have been loaded or are not relevant.
        matches = await FilterMatches(matches);

        /*await Parallel.ForEachAsync(matches, new ParallelOptions { MaxDegreeOfParallelism = 4 },
            async (match, cancelToken) => { await GetMatchDetails(match); });*/

        foreach (var match in matches)
        {
            await GetMatchDetails(match);
        }
    }

    private async Task<List<Match>> FilterMatches(List<Match> matches)
    {
        // filter only relevant, parsed matches.
        matches = matches.Where(x => x.LobbyType
                                         is DotaEnums.LobbyType.BattleCup
                                         or DotaEnums.LobbyType.Ranked
                                         or DotaEnums.LobbyType.Normal
                                         or DotaEnums.LobbyType.Practice &&
                                     (x.Skill != null || x.GameMode == DotaEnums.GameMode.CaptainsMode))
            .ToList();


        var matchIds = matches.Select(x => x.MatchId).ToArray();

        var unparsedMatchIds = _context.UnParsedMatches.Where(x => (x.FailureReason != DotaDataEnums.MatchParseFailureReason.UnparsedReplay) && matchIds.Contains(x.MatchId)).Select(x => x.MatchId)
            .ToArray();

        // Check to see if these matches already exist / have been completed loaded.
        var existingMatches = await _context.Matches.Where(x => matchIds.Contains(x.MatchId) && x.Loaded)
            .Select(x => x.MatchId).ToListAsync();

        matches = matches.Where(x => !existingMatches.Contains(x.MatchId) && !unparsedMatchIds.Contains(x.MatchId))
            .ToList();

        return matches;
    }

    private async Task GetMatchDetails(Match match)
    {
        // Open dota will return 429s if we don't throttle our match parsing :(


        try
        {
            var response = await
                _httpClient.GetAsync($"/api/matches/{match.MatchId}?api_key={_config["OpenDota:ApiKey"]}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            dynamic matchDetails = JsonConvert.DeserializeObject(content)!;

            // Do not store matches that have bad data
            if (matchDetails["players"]?[0]["damage"] == null)
            {
                var unParsedMatch = await _context.UnParsedMatches.FirstOrDefaultAsync(x => x.MatchId == match.MatchId);

                if (unParsedMatch != null)
                {
                    await _context.UnParsedMatches.Upsert(new UnParsedMatch { MatchId = match.MatchId, FailureReason = DotaDataEnums.MatchParseFailureReason.MissingReplay, ParseRequestSent = true}).On(x => x.MatchId)
                        .RunAsync();
                    return;
                }
                
                await _context.UnParsedMatches.Upsert(new UnParsedMatch { MatchId = match.MatchId, FailureReason = DotaDataEnums.MatchParseFailureReason.UnparsedReplay, ParseRequestSent = false}).On(x => x.MatchId)
                    .RunAsync();

                return;
            }

            match.BarracksStatusDire = matchDetails["barracks_status_dire"];
            match.BarracksStatusRadiant = matchDetails["barracks_status_radiant"];
            match.Cluster = matchDetails["cluster"];
            match.DireScore = matchDetails["dire_score"];
            match.DireTeamId = matchDetails["dire_team_id"];
            match.Engine = matchDetails["engine"];
            match.FirstBloodTime = matchDetails["first_blood_time"];
            match.HumanPlayers = matchDetails["human_players"];
            match.Leagueid = matchDetails["leagueid"];
            match.MatchSeqNum = matchDetails["match_seq_num"];
            match.NegativeVotes = matchDetails["negative_votes"];
            match.PositiveVotes = matchDetails["positive_votes"];
            match.RadiantGoldAdv = matchDetails["radiant_gold_adv"]?.ToObject<long[]>() ?? Array.Empty<long>();
            match.RadiantScore = matchDetails["radiant_score"];
            match.RadiantTeamId = matchDetails["radiant_team_id"];
            match.RadiantXpAdv = matchDetails["radiant_xp_adv"]?.ToObject<long[]>() ?? Array.Empty<long>();
            match.Skill = matchDetails["skill"];
            match.TowerStatusDire = matchDetails["tower_status_dire"];
            match.TowerStatusRadiant = matchDetails["tower_status_radiant"];
            match.ReplaySalt = matchDetails["replay_salt"];
            match.SeriesId = matchDetails["series_id"];
            match.SeriesType = matchDetails["series_type"];
            match.Patch = matchDetails["patch"];
            match.Region = matchDetails["region"];
            match.Throw = matchDetails["throw"];
            match.Loss = matchDetails["loss"];
            match.ReplayUrl = matchDetails["replay_url"];
            match.CreatedAt ??= DateTime.Now.ToUniversalTime();
            match.UpdatedAt = DateTime.Now.ToUniversalTime();

            await _context.Matches.Upsert(match).On(x => x.MatchId).RunAsync();

            foreach (JObject player in matchDetails["players"])
            {
                // Ignore anonymous users.
                if (player["account_id"]?.Value<long?>() == null)
                {
                    continue;
                }

                var playerMatch = new PlayerMatch();
                playerMatch.LeagueId = match.Leagueid ?? -1;
                playerMatch.MatchId = match.MatchId;
                playerMatch.Skill = match.Skill;
                playerMatch.PlayerId = player["account_id"]?.Value<long?>() ?? 0;
                playerMatch.CreatedAt ??= DateTime.Now.ToUniversalTime();
                playerMatch.UpdatedAt = DateTime.Now.ToUniversalTime();
                playerMatch.PlayerSlot = player["player_slot"]?.Value<int?>() ?? 0;
                playerMatch.AbilityUpgradesArr =
                    player["ability_upgrades_arr"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.Assists = player["assists"]?.Value<int?>() ?? 0;
                playerMatch.Backpack0 = player["backpack_0"]?.Value<int?>();
                playerMatch.Backpack1 = player["backpack_1"]?.Value<int?>();
                playerMatch.Backpack2 = player["backpack_2"]?.Value<int?>();
                playerMatch.Backpack3 = player["backpack_3"]?.Value<int?>();
                playerMatch.CampsStacked = player["camps_stacked"]?.Value<int?>() ?? 0;
                playerMatch.CreepsStacked = player["creeps_stacked"]?.Value<int?>() ?? 0;
                playerMatch.Deaths = player["deaths"]?.Value<int?>() ?? 0;
                playerMatch.Denies = player["denies"]?.Value<int?>() ?? 0;
                playerMatch.DnT = player["dn_t"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.FirstbloodClaimed = player["firstblood_claimed"]?.Value<int?>() == 1;
                playerMatch.Gold = player["gold"]?.Value<int?>() ?? 0;
                playerMatch.GoldSpent = player["gold_spent"]?.Value<int?>() ?? 0;
                playerMatch.GoldPerMin = player["gold_per_min"]?.Value<int?>() ?? 0;
                playerMatch.GoldT = player["gold_t"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.HeroDamage = player["hero_damage"]?.Value<long?>() ?? 0;
                playerMatch.HeroHealing = player["hero_healing"]?.Value<long?>() ?? 0;
                playerMatch.HeroId = (DotaEnums.Hero)(player["hero_id"]?.Value<int?>() ?? 0);
                playerMatch.Item0 = (DotaEnums.Item?)player["item_0"]?.Value<long?>();
                playerMatch.Item1 = (DotaEnums.Item?)player["item_1"]?.Value<long?>();
                playerMatch.Item2 = (DotaEnums.Item?)player["item_2"]?.Value<long?>();
                playerMatch.Item3 = (DotaEnums.Item?)player["item_3"]?.Value<long?>();
                playerMatch.Item4 = (DotaEnums.Item?)player["item_4"]?.Value<long?>();
                playerMatch.Item5 = (DotaEnums.Item?)player["item_5"]?.Value<long?>();
                playerMatch.ItemNeutral = (DotaEnums.Item?)player["item_neutral"]?.Value<long?>();
                playerMatch.Kills = player["kills"]?.Value<int?>() ?? 0;
                playerMatch.LastHits = player["last_hits"]?.Value<int?>() ?? 0;
                playerMatch.LeaverStatus = player["leaver_status"]?.Value<int?>() ?? 0;
                playerMatch.Level = player["level"]?.Value<int?>() ?? 0;
                playerMatch.LhT = player["lh_t"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.NetWorth = player["net_worth"]?.Value<long?>() ?? 0;
                playerMatch.ObsPlaced = player["obs_placed"]?.Value<int?>() ?? 0;
                playerMatch.PartyId = player["party_id"]?.Value<int?>();
                playerMatch.PartySize = player["party_size"]?.Value<int?>();
                playerMatch.Pings = player["pings"]?.Value<int?>() ?? 0;
                playerMatch.PredVict = player["pred_vict"]?.Value<bool?>() ?? false;
                playerMatch.Randomed = player["randomed"]?.Value<bool?>() ?? false;
                playerMatch.Repicked = player["repicked"]?.Value<bool?>() ?? false;
                playerMatch.RoshansKilled = player["roshans_killed"]?.Value<int?>() ?? 0;
                playerMatch.RunePickups = player["rune_pickups"]?.Value<int?>() ?? 0;
                playerMatch.SenPlaced = player["sen_placed"]?.Value<int?>() ?? 0;
                playerMatch.Stuns = player["stuns"]?.Value<decimal?>() ?? 0;
                playerMatch.TeamfightParticipation = player["teamfight_participation"]?.Value<decimal?>() ?? 0;
                playerMatch.Times = player["times"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.TowerDamage = player["tower_damage"]?.Value<long?>() ?? 0;
                playerMatch.TowersKilled = player["towers_killed"]?.Value<int?>() ?? 0;
                playerMatch.XpPerMin = player["xp_per_min"]?.Value<int?>() ?? 0;
                playerMatch.XpT = player["xp_t"]?.ToObject<int[]>() ?? Array.Empty<int>();
                playerMatch.StartTime = match.StartTime ?? 0;
                playerMatch.Duration = player["duration"]?.Value<long?>() ?? 0;
                ;
                playerMatch.Cluster = player["cluster"]?.Value<int?>();
                playerMatch.LobbyType = match.LobbyType;
                playerMatch.Region = match.Region;
                playerMatch.Win = player["win"]?.Value<bool?>() ?? false;
                playerMatch.Lose = player["lose"]?.Value<bool?>() ?? false;
                playerMatch.TotalGold = player["total_gold"]?.Value<long?>() ?? 0;
                playerMatch.TotalXp = player["total_xp"]?.Value<long?>() ?? 0;
                playerMatch.KillsPerMin = player["kills_per_min"]?.Value<decimal>() ?? 0;
                playerMatch.Kda = player["kda"]?.Value<decimal>() ?? 0;
                playerMatch.Abandons = player["abandons"]?.Value<int?>() ?? 0;
                playerMatch.NeutralKills = player["neutral_kills"]?.Value<int?>() ?? 0;
                playerMatch.TowerKills = player["tower_kills"]?.Value<int?>() ?? 0;
                playerMatch.CourierKills = player["courier_kills"]?.Value<int?>() ?? 0;
                playerMatch.LaneKills = player["lane_kills"]?.Value<int?>() ?? 0;
                playerMatch.ObserverKills = player["observer_kills"]?.Value<int?>() ?? 0;
                playerMatch.RoshanKills = player["roshan_kills"]?.Value<int?>() ?? 0;
                playerMatch.SentryKills = player["sentry_kills"]?.Value<int?>() ?? 0;
                playerMatch.AncientKills = player["ancient_kills"]?.Value<int?>() ?? 0;
                playerMatch.BuyBackCount = player["buyback_count"]?.Value<int?>() ?? 0;
                playerMatch.ObserverUses = player["observer_uses"]?.Value<int?>() ?? 0;
                playerMatch.SentryUses = player["sentry_uses"]?.Value<int?>() ?? 0;
                playerMatch.LaneEfficiencyPct = player["lane_efficiency_pct"]?.Value<int?>() ?? 0;
                playerMatch.Lane = (DotaEnums.Lane?)player["lane"]?.Value<int?>() ?? DotaEnums.Lane.Unknown;
                playerMatch.LaneRole = player["lane_role"]?.Value<int?>() ?? 0;
                playerMatch.IsRoaming = player["is_roaming"]?.Value<bool?>() ?? false;
                playerMatch.ActionsPerMinute = player["actions_per_min"]?.Value<long?>() ?? 0;
                playerMatch.LifeStateDead = player["life_state_dead"]?.Value<int?>() ?? 0;
                playerMatch.RankTier = player["rank_tier"]?.Value<int?>() ?? 0;

                playerMatch.MultiKills2 = player["multi_kills"]?.Value<JObject?>()?["2"]?.Value<int?>() ?? 0;
                playerMatch.MultiKills3 = player["multi_kills"]?.Value<JObject?>()?["3"]?.Value<int?>() ?? 0;
                playerMatch.MultiKills4 = player["multi_kills"]?.Value<JObject?>()?["4"]?.Value<int?>() ?? 0;
                playerMatch.MultiKills5 = player["multi_kills"]?.Value<JObject?>()?["5"]?.Value<int?>() ?? 0;


                if (player["connection_log"] != null)
                {
                    foreach (var connection in player["connection_log"])
                    {
                        if (connection["event"]?.Value<string>() != "disconnect")
                        {
                            continue;
                        }

                        playerMatch.DisconnectCount++;
                        playerMatch.DisconnectTotalTime += connection["time"]?.Value<int?>() ?? 0;
                    }
                }

                await _context.PlayerMatches.Upsert(playerMatch).On(x => new { x.PlayerId, x.MatchId }).RunAsync();

                playerMatch.AbilityUses = new List<PlayerMatchAbility>();
                if (player["ability_uses"] != null)
                {
                    foreach (dynamic itemRaw in player["ability_uses"])
                    {
                        var item = new PlayerMatchAbility();
                        item.Ability = itemRaw.Name;
                        item.Count = itemRaw.Value;
                        item.MatchId = match.MatchId;
                        item.PlayerId = playerMatch.PlayerId;
                        playerMatch.AbilityUses.Add(item);
                    }

                    await _context.PlayerMatchAbilities.UpsertRange(playerMatch.AbilityUses)
                        .On(x => new { x.PlayerId, x.MatchId, x.Ability }).RunAsync();
                }

                playerMatch.Actions = new List<PlayerMatchAction>();
                if (player["ability_uses"] != null)
                {
                    foreach (dynamic itemRaw in player["actions"])
                    {
                        var item = new PlayerMatchAction();
                        item.Action = itemRaw.Name;
                        item.Count = itemRaw.Value;
                        item.MatchId = match.MatchId;
                        item.PlayerId = playerMatch.PlayerId;
                        playerMatch.Actions.Add(item);
                    }
                }

                await _context.PlayerMatchActions.UpsertRange(playerMatch.Actions)
                    .On(x => new { x.PlayerId, x.MatchId, x.Action }).RunAsync();

                playerMatch.ItemUses = new List<PlayerMatchItemUse>();
                foreach (dynamic itemRaw in player["item_uses"])
                {
                    var item = new PlayerMatchItemUse();
                    item.Item = itemRaw.Name;
                    item.Uses = itemRaw.Value;
                    item.MatchId = match.MatchId;
                    item.PlayerId = playerMatch.PlayerId;
                    playerMatch.ItemUses.Add(item);
                }

                await _context.PlayerMatchItemUses.UpsertRange(playerMatch.ItemUses)
                    .On(x => new { x.PlayerId, x.MatchId, x.Item }).RunAsync();

                playerMatch.FirstPurchases = new List<PlayerMatchItemFirstPurchase>();
                foreach (dynamic itemRaw in player["first_purchase_time"])
                {
                    var item = new PlayerMatchItemFirstPurchase();
                    item.Item = itemRaw.Name;
                    item.Time = itemRaw.Value as long? ?? 0;
                    item.MatchId = match.MatchId;
                    item.PlayerId = playerMatch.PlayerId;
                    playerMatch.FirstPurchases.Add(item);
                }

                await _context.PlayerMatchItemFirstPurchases.UpsertRange(playerMatch.FirstPurchases)
                    .On(x => new { x.PlayerId, x.MatchId, x.Item }).RunAsync();
            }

            match.Loaded = true;
            await _context.Matches.Upsert(match).On(x => x.MatchId).RunAsync();
        }
        catch (HttpRequestException)
        {
            _logger.LogError("Could not fetch match with id {MatchId}. Adding to unparsed match list", match.MatchId);
            
            await _context.UnParsedMatches.Upsert(new UnParsedMatch { MatchId = match.MatchId, FailureReason = DotaDataEnums.MatchParseFailureReason.InternalServerError, ParseRequestSent = false}).On(x => x.MatchId)
                .RunAsync();
        }
    }

    private async Task ParseReplays()
    {
        var matches = await _context.UnParsedMatches.Where(x =>
            x.FailureReason == DotaDataEnums.MatchParseFailureReason.UnparsedReplay && x.ParseRequestSent == false).ToListAsync();
        
        foreach (var match in matches)
        {
            var response = await
                _httpClient.PostAsync($"/api/request/{match.MatchId}?api_key={_config["OpenDota:ApiKey"]}", null);
            
            response.EnsureSuccessStatusCode();
            
            await _context.UnParsedMatches.Upsert(new UnParsedMatch { MatchId = match.MatchId, FailureReason = DotaDataEnums.MatchParseFailureReason.UnparsedReplay, ParseRequestSent = true}).On(x => x.MatchId)
                .RunAsync();
          
            Thread.Sleep(100);
        }
    }
    
}