using RD2LPowerRankings.Modules.GoogleSheets;
using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Services.PlayerDataSource;

public class RD2LPlayerDataSource : IPlayerDataSource
{
    private readonly ILogger<RD2LPlayerDataSource> _logger;
    private readonly IGoogleSheetsService _sheetsService;

    public RD2LPlayerDataSource(ILogger<RD2LPlayerDataSource> logger, IGoogleSheetsService sheetsService)
    {
        _logger = logger;
        _sheetsService = sheetsService;
    }

    public List<PlayerDataSourcePlayer> GetPlayers(string sheetId)
    {
        _logger.LogInformation("Loading player(s) using sheet {SheetId}", sheetId);

        var players = GetPlayersFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangePlayers);

        var captains = GetCaptainsFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangeCaptains);

        _logger.LogInformation("Loaded {Count} player(s) info using sheet {SheetId}", players.Count, sheetId);

        return players.Concat(captains).ToList();
    }

    public List<PlayerDataSourceTeam> GetTeams(string sheetId)
    {
        _logger.LogInformation("Loading teams using sheet {SheetId}", sheetId);

        var players = GetPlayersFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangePlayers);

        var captains = GetCaptainsFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangeCaptains);

        var teams = GetTeams(players, captains);

        _logger.LogInformation("Loaded {Count} teams using sheet {SheetId}", teams.Count, sheetId);

        return teams;
    }

    private List<PlayerDataSourcePlayer> GetPlayersFromSheet(string sheetId, string sheetRange)
    {
        var players = new List<PlayerDataSourcePlayer>();

        var responsePlayers = _sheetsService.Service.Spreadsheets.Values
            .Get(sheetId, sheetRange).Execute()
            .Values ?? new List<IList<object>>();

        foreach (var row in responsePlayers.Where(x => x.Count > 3))
        {
            var playerId = row[(int)PlayerColumnsSeason32.Dotabuff]?.ToString()
                ?.Replace(@"https://www.dotabuff.com/players/", "");
            var playerName = row[(int)PlayerColumnsSeason32.Name]?.ToString() ??
                             PlayerDataSourceConstants.DefaultPlayerName;

            var captainName = row[(int)PlayerColumnsSeason32.PickedBy]?.ToString() ??
                              PlayerDataSourceConstants.DefaultPlayerName;

            var cost = 0; //Convert.ToDecimal(row[(int)PlayerColumnsSeason31.Cost]?.ToString() ?? null);

            var playerStatement = row.Count <= (int)PlayerColumnsSeason32.Statement
                ? ""
                : row[(int)PlayerColumnsSeason32.Statement]?.ToString() ?? "";

            //   var estimatedValue = Convert.ToDecimal(row[(int)PlayerColumnsSeason32.Value]?.ToString() ?? null);

            if (long.TryParse(playerId, out var playerIdParsed))
            {
                players.Add(new PlayerDataSourcePlayer
                {
                    Id = playerIdParsed, Name = playerName, IsCaptain = false, CaptainName = captainName, Cost = cost,
                    EstimatedValue = 0M, PlayerStatement = playerStatement
                });
            }

            _logger.LogInformation($"Added {playerName}");
        }

        return players;
    }

    private List<PlayerDataSourcePlayer> GetCaptainsFromSheet(string sheetId, string sheetRange)
    {
        var players = new List<PlayerDataSourcePlayer>();

        var responsePlayers = _sheetsService.Service.Spreadsheets.Values
            .Get(sheetId, sheetRange).Execute()
            .Values ?? new List<IList<object>>();

        foreach (var row in responsePlayers.Where(x => x.Count > 3))
        {
            var playerId = row[(int)CaptainsColumnsSeason32.Dotabuff]?.ToString()
                ?.Replace(@"https://www.dotabuff.com/players/", "");
            var playerName = row[(int)CaptainsColumnsSeason32.Name]?.ToString() ??
                             PlayerDataSourceConstants.DefaultPlayerName;

            var playerStatement = row[(int)CaptainsColumnsSeason32.Statement]?.ToString() ?? "";

            if (long.TryParse(playerId, out var playerIdParsed))
            {
                players.Add(new PlayerDataSourcePlayer
                {
                    Id = playerIdParsed, Name = playerName, IsCaptain = true, CaptainName = playerName,
                    PlayerStatement = playerStatement
                });
            }
        }

        return players;
    }

    private List<PlayerDataSourceTeam> GetTeams(List<PlayerDataSourcePlayer> players,
        List<PlayerDataSourcePlayer> captains)
    {
        var teams = new List<PlayerDataSourceTeam>();

        foreach (var captain in captains)
        {
            var team = new PlayerDataSourceTeam();
            team.Name = captain.Name;
            team.Players = new List<PlayerDataSourcePlayer>();
            team.Players.Add(new PlayerDataSourcePlayer { IsCaptain = true, Id = captain.Id, Name = captain.Name });

            var teamPlayers = players.Where(x => x.CaptainName == captain.Name);

            team.Players.AddRange(teamPlayers);

            teams.Add(team);
        }

        return teams;
    }
}