using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Services.PlayerDataSource;

namespace RD2LPowerRankings.Services.DotaDataSource;

public class OpenDotaExtendedDataSource : IDotaExtendedDataSource
{
    private readonly IConfiguration _config;
    private readonly DotaDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenDotaExtendedDataSource> _logger;
    private readonly IPlayerDataSource _playerDataSource;

    public OpenDotaExtendedDataSource(
        ILogger<OpenDotaExtendedDataSource> logger,
        IConfiguration config,
        IPlayerDataSource playerDataSource,
        DotaDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _playerDataSource = playerDataSource;
        _context = context;
        _config = config;
        _httpClient = httpClientFactory.CreateClient(nameof(OpenDotaExtendedDataSource));
        _httpClient.Timeout = TimeSpan.FromMinutes(60);
        _httpClient.BaseAddress = new Uri(_config["OpenDota:ApiUrl"]);
    }


    public async Task LoadExtendedPlayerData(string sheetId)
    {
        var players = _playerDataSource.GetPlayers(sheetId);

        foreach (var player in players)
        {
            await SavePlayer(player.Id);

            await SaveWords(player.Id);
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

    private async Task SavePlayer(long playerId)
    {
        var player = await _context.Players.FirstOrDefaultAsync(x => x.PlayerId == playerId) ??
                     new Player { PlayerId = playerId };

        var response = await
            _httpClient.GetAsync($"/api/players/{playerId}/?api_key={_config["OpenDota:ApiKey"]}");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        dynamic playerRaw = JsonConvert.DeserializeObject(content)!;

        player.Cheese = playerRaw["profile"]["cheese"];
        player.Loccountrycode = playerRaw["profile"]["loccountrycode"];
        player.IsContributor = playerRaw["profile"]["is_contributor"];
        player.RankTier = playerRaw["rank_tier"];
        if (playerRaw["mmr_estimate"] != null && playerRaw["mmr_estimate"]["estimate"] != null)
        {
            player.MmrEstimate = playerRaw["mmr_estimate"]["estimate"];
        }

        player.LeaderboardRank = playerRaw["leaderboard_rank"];
        player.CreatedAt ??= DateTime.Now.ToUniversalTime();
        player.UpdatedAt = DateTime.Now.ToUniversalTime();

        await _context.Players.Upsert(player).On(x => x.PlayerId).RunAsync();
    }
}