using System.Net.Http.Headers;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Helpers;
using RD2LPowerRankings.Modules.Dota.Model;
using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Services.DotaRanking;

public class OpenAIService : IOpenAIService

{
    private readonly IConfiguration _config;
    private readonly DotaDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;


    public OpenAIService(ILogger<OpenAIService> logger,
        IConfiguration config,
        DotaDbContext context,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        ;
        _context = context;
        _config = config;
        _httpClient = httpClientFactory.CreateClient(nameof(OpenAIService));
        _httpClient.Timeout = TimeSpan.FromMinutes(60);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);
        _httpClient.BaseAddress = new Uri(_config["OpenAI:ApiUrl"] ?? "");
    }

    public async Task<List<PowerRankedPlayer>> GeneratePlayerReviews(List<PowerRankedPlayer> players)
    {
        foreach (var player in players)
        {
            var dbPlayer = await _context.Players.FirstAsync(x => x.PlayerId == player.PlayerId);

            if (dbPlayer.Description != null)
            {
                player.PlayerReview.Result = dbPlayer.Description;
                continue;
            }

            player.PlayerReview = GeneratePrompts(player);

            player.PlayerReview.Result = await GetReviewFromOpenAi(player.PlayerReview.Prompt);

            dbPlayer.Description = player.PlayerReview.Result;

            dbPlayer.Description = CleanUpDescription(dbPlayer.Description);

            await _context.Players.Upsert(dbPlayer).On(x => x.PlayerId).RunAsync();
        }

        return players;
    }

    private PlayerReview GeneratePrompts(PowerRankedPlayer player)
    {
        if (player.RankTier.HasValue)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.GenerateRankWords(player.RankTier ?? 0)
                .PickRandom());
        }

        if (player.ToxicityRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.ToxicityTier1Words.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixToxicBadWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.WordUsage.PickRandom()} times");
        }
        else if (player.ToxicityRank <= 15)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.ToxicityTier2Words.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixToxicBadWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.WordUsage.PickRandom()} times");
        }

        if (player.SafelaneRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.Tier1SafeLaneWords.PickRandom());
        }

        if (player.MidlaneRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.Tier1MidLaneWords.PickRandom());
        }

        if (player.OfflaneRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.Tier1OffLaneWords.PickRandom());
        }

        if (player.SoftSupportRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.Tier1SoftSupportWords.PickRandom());
        }

        if (player.HardSupportRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.Tier1HardSupportWords.PickRandom());
        }

        if (player.Awards.Any(x => x.Name == "Wholesome Player"))
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.WholesomeWords.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixWholesomeWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.WordUsage.PickRandom()} times");
        }


        var hero = player.RespectBans.FirstOrDefault(x => x == player.Heroes.MaxBy(y => y.MatchesPlayed)?.HeroId);
        if (hero != 0)
        {
            player.PlayerReview.Attributes.Add(
                $"{OpenAIPlayerSentenceBuilders.GoodHeroWords.PickRandom()} {Enum.GetName(hero).Replace("_", " ")}");

            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.GetHeroDescriptionWords(hero).PickRandom());
        }

        if (player.PlayerReview.Attributes.Count <= 1)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.UnknownPlayerWords.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                OpenAIPlayerSentenceBuilders.UnknownPlayerEndingSentences.PickRandom());
        }

        player.PlayerReview.Attributes = player.PlayerReview.Attributes.Shuffle().ToList();
        player.PlayerReview.ReviewPrefixAdjective = OpenAIPlayerSentenceBuilders.ReviewPrefixWords.PickRandom();

        player.PlayerReview.Prompt =
            $"Write a {player.PlayerReview.ReviewPrefixAdjective} five sentence power ranking review on a dota2 player named {player.DraftName} " +
            $"who has the following attributes: {string.Join(",", player.PlayerReview.Attributes)}. {string.Join(",", player.PlayerReview.EndingSentences)}";
        return player.PlayerReview;
    }

    private async Task<string> GetReviewFromOpenAi(string prompt)
    {
        var httpContent = new StringContent(JsonConvert.SerializeObject(new CompletionRequest
        {
            Prompt = prompt,
            Model = "text-davinci-003",
            MaxTokens = 1000,
            Temperature = 1.0
        }), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/completions", httpContent);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        dynamic rawResults = JsonConvert.DeserializeObject(content)!;
        return rawResults["choices"][0]["text"];
    }

    private string CleanUpDescription(string description)
    {
        description = description.Replace("1.", "")
            .Replace("2.", "".Replace("3.", "").Replace("4.", "").Replace("5.", ""))
            .Replace("1)", "").Replace("2)", "".Replace("3)", "").Replace("4)", "").Replace("5)", ""));
        return description;
    }
}