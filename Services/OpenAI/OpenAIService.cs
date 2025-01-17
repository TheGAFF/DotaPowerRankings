﻿using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RD2LPowerRankings.Database.Dota;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Helpers;
using RD2LPowerRankings.Modules.Dota.Model;

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

    public async Task<List<PowerRankedTeam>> GenerateTeamReviews(List<PowerRankedTeam> teams, string seasonName)
    {
        foreach (var team in teams)
        {
            var captainId = team.Players.First(y => y.IsCaptain).PlayerId;
            var dbTeam = await _context.Teams.FirstOrDefaultAsync(x =>
                             x.TeamCaptainId == captainId &&
                             x.SeasonName == seasonName) ??
                         new Team
                         {
                             TeamCaptainId = team.Players.First(y => y.IsCaptain).PlayerId, SeasonName = seasonName,
                             CreatedAt = DateTime.Now.ToUniversalTime()
                         };

            team.Name = dbTeam.TeamName ?? team.Name;

            team.TeamReview = GenerateTeamPrompt(team, teams.Count);

            if (dbTeam.Description != null)
            {
                team.TeamReview.Result = dbTeam.Description;
                continue;
            }


            team.TeamReview.Result = await GetReviewFromKoboldAi(team.TeamReview.Prompt, 1024);

            dbTeam.Description = team.TeamReview.Result;

            dbTeam.UpdatedAt = DateTime.Now.ToUniversalTime();
            await _context.Teams.Upsert(dbTeam).On(x => new { x.TeamCaptainId, x.SeasonName }).RunAsync();
        }

        return teams;
    }

    public async Task<List<PowerRankedPlayer>> GeneratePlayerReviews(List<PowerRankedPlayer> players, string seasonName)
    {
        foreach (var player in players)
        {
            var playerDescription =
                await _context.PlayerDescriptions.FirstOrDefaultAsync(x =>
                    x.PlayerId == player.PlayerId && x.SeasonName == seasonName);

            if (playerDescription != null)
            {
                player.PlayerReview.Result = playerDescription.Description;
                continue;
            }

            playerDescription = new PlayerDescription();
            playerDescription.PlayerId = player.PlayerId;
            playerDescription.UpdatedAt = DateTime.Now.ToUniversalTime();

            player.PlayerReview = GeneratePlayerPrompt(player);

            player.PlayerReview.Result = await GetReviewFromKoboldAi(player.PlayerReview.Prompt, 384);

            playerDescription.Description = player.PlayerReview.Result ?? "";

            playerDescription.SeasonName = seasonName;

            playerDescription.Prompt = player.PlayerReview.Prompt;

            playerDescription.Description = CleanUpDescription(playerDescription.Description) ?? "";

            await _context.PlayerDescriptions.Upsert(playerDescription).On(x => new { x.PlayerId, x.SeasonName })
                .RunAsync();
        }

        return players;
    }

    private PlayerReview GeneratePlayerPrompt(PowerRankedPlayer player)
    {
        if (player.RankTier.HasValue)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders
                .GeneratePlayerRankWords(player.RankTier ?? 0)
                .PickRandom());
        }

        if (player.ToxicityRank <= 5)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.ToxicityTier1Words.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixToxicBadWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.PlayerWordUsage.PickRandom()} times");
        }
        else if (player.ToxicityRank <= 15)
        {
            player.PlayerReview.Attributes.Add(OpenAIPlayerSentenceBuilders.ToxicityTier2Words.PickRandom());
            player.PlayerReview.EndingSentences.Add(
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixToxicBadWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.PlayerWordUsage.PickRandom()} times");
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
                $"Use the word {OpenAIPlayerSentenceBuilders.SuffixWholesomeWords.PickRandom()} at least {OpenAIPlayerSentenceBuilders.PlayerWordUsage.PickRandom()} times");
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
            $"Write a {player.PlayerReview.ReviewPrefixAdjective} power ranking review on a dota2 player named {player.DraftName} " +
            $"who has the following attributes: {string.Join(",", player.PlayerReview.Attributes)}. {string.Join("", player.PlayerReview.EndingSentences)}";

        if (!string.IsNullOrWhiteSpace(player.Statement))
        {
            player.PlayerReview.Prompt +=
                $". Use this in your review about what the player says about themselves: '{player.Statement}'";
        }

        return player.PlayerReview;
    }

    private TeamReview GenerateTeamPrompt(PowerRankedTeam team, int teamCount)
    {
        foreach (var player in team.Players)
        {
            if (player.IsCaptain)
            {
                player.PlayerReview.TeamAttributes.Add(OpenAIPlayerSentenceBuilders.TeamCaptainWords.PickRandom());
            }

            if (player.RankTier.HasValue)
            {
                player.PlayerReview.TeamAttributes.Add(OpenAIPlayerSentenceBuilders
                    .GenerateTeamRankWords(player.RankTier ?? 0, team.Players.Select(x => x.RankTier))
                    .PickRandom());
            }

            if (player.PlayerReview.TeamAttributes.Count <= 1)
            {
                var hero = player.RespectBans.FirstOrDefault(
                    x => x == player.Heroes.MaxBy(y => y.MatchesPlayed)?.HeroId);
                if (hero != 0)
                {
                    player.PlayerReview.TeamAttributes.Add(
                        $"{OpenAIPlayerSentenceBuilders.GoodHeroWords.PickRandom()} {Enum.GetName(hero).Replace("_", " ")}");

                    player.PlayerReview.TeamAttributes.Add(OpenAIPlayerSentenceBuilders.GetHeroDescriptionWords(hero)
                        .PickRandom());
                }
            }

            if (player.PlayerReview.TeamAttributes.Count <= 1)
            {
                player.PlayerReview.TeamAttributes.Add(OpenAIPlayerSentenceBuilders.UnknownPlayerWords.PickRandom());
            }


            player.PlayerReview.TeamPrompt =
                $"\n{player.DraftName}: {string.Join(",", player.PlayerReview.TeamAttributes)}.";
        }

        if (team.Awards.Count > 0)
        {
            team.TeamReview.Attributes.AddRange(team.Awards.Select(x => x.Name.ToLower())
                .Take(team.Awards.Count <= 3 ? team.Awards.Count : 3));
        }


        team.TeamReview.Attributes.Add(
            OpenAIPlayerSentenceBuilders.GetTeamRankWords(team.Rank, team.Rank * 1M / teamCount * 1M).PickRandom());


        team.TeamReview.EndingSentences.Add(OpenAIPlayerSentenceBuilders.TeamEndingSentences.PickRandom());


        team.TeamReview.ReviewPrefixAdjective = OpenAIPlayerSentenceBuilders.ReviewPrefixWords.PickRandom();
        team.TeamReview.Prompt =
            $"Write a {team.TeamReview.ReviewPrefixAdjective} 700-900 word dota2 power ranking team review for a team named '{team.Name}' " +
            "that has the following players with the following attributes:" +
            $"{string.Join("", team.Players.SelectMany(x => x.PlayerReview.TeamPrompt))}" +
            $"\nThe team has the following attributes: {string.Join(",", team.TeamReview.Attributes)}. {string.Join("", team.TeamReview.EndingSentences)}";

        return team.TeamReview;
    }

    /// <summary>
    ///     Retrieves a text review generated by OpenAI's language model based on the given prompt.
    /// </summary>
    /// <param name="prompt">The input prompt to provide to the OpenAI model.</param>
    /// <param name="maxTokens">
    ///     The maximum number of tokens (words-ish) to generate in the response. If the tokens in your prompt
    ///     + maxTokens is greater than 2048 (max context size for a lot of models), then expect weird results since it will
    ///     cut off your prompt.
    /// </param>
    private async Task<string> GetReviewFromOpenAi(string prompt, int maxTokens)
    {
        var random = new Random();
        var temperature = random.NextDouble() * 0.2 + 0.8;

        var httpContent = new StringContent(JsonConvert.SerializeObject(new CompletionRequest
        {
            Prompt = prompt,
            Model = "estopianmaid-13b.Q5_K_M.gguf",
            MaxTokens = maxTokens,
            Temperature = temperature
        }), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/v1/completions", httpContent);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            dynamic rawResults = JsonConvert.DeserializeObject(content)!;
            return rawResults["choices"][0]["text"];
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private async Task<string> GetReviewFromKoboldAi(string prompt, int maxTokens)
    {
        var random = new Random();
        var temperature = random.NextDouble() * 0.2 + 0.9;

        var httpContent = new StringContent(JsonConvert.SerializeObject(new KoBoldCompletionRequest
        {
            Prompt = $"<|im_end|>\\n<|im_start|>user\\n{prompt}<|im_end|>\\n<|im_start|>assistant\\n",
            MaxContextLength = 2048,
            MaxLength = maxTokens,
            Temperature = temperature,
            Quiet = false,
            RepPen = 1.1,
            RepPenRange = 256,
            RepPenSlope = 1,
            Tfs = 1,
            TopA = 0,
            TopK = 100,
            TopP = 1,
            Typical = 1,
            StopSequence = ["<|im_end|>\\n<|im_start|>user", "<|im_end|>\\n<|im_start|>assistant"]
        }), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/v1/generate", httpContent);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            dynamic rawResults = JsonConvert.DeserializeObject(content)!;
            return rawResults["results"][0]["text"];
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    private string? CleanUpDescription(string? description)
    {
        return description == null ? description : Regex.Replace(description, @"\d[\.\)]", "");
    }
}