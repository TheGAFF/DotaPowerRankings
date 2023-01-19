using System.Net.Http.Headers;
using RD2LPowerRankings.Database.Dota;
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
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config["OpenAI:ApiKey"]);
        _httpClient.BaseAddress = new Uri(_config["OpenAI:ApiUrl"] ?? "");
    }

    public PowerRankedLeague GeneratePlayerReviews(PowerRankedLeague league)
    {
        var players = league.Divisions.SelectMany(x => x.Teams).SelectMany(x => x.Players).ToList();
        foreach (var player in players)
        {
            player.PlayerReview = GivePlayerReview(player, players);
        }


        return league;
    }

    private PlayerReview GivePlayerReview(PowerRankedPlayer player, IList<PowerRankedPlayer> players)
    {
        var review = new PlayerReview();


        return player.PlayerReview;
    }
}