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

        var players = GetPlayersFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangePlayers, false);

        var captains = GetPlayersFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangeCaptains, true);

        _logger.LogInformation("Loaded {Count} player(s) info using sheet {SheetId}", players.Count, sheetId);

        return players.Concat(captains).ToList();
    }

    public List<PlayerDataSourceTeam> GetTeams(string sheetId)
    {
        _logger.LogInformation("Loading teams using sheet {SheetId}", sheetId);

        var captains = GetPlayersFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangeCaptains, true);

        var teams = GetTeamsFromSheet(sheetId, PlayerDataSourceConstants.RD2LSheetRangeTeams, captains);

        _logger.LogInformation("Loaded {Count} teams using sheet {SheetId}", teams.Count, sheetId);

        return teams;
    }

    private List<PlayerDataSourcePlayer> GetPlayersFromSheet(string sheetId, string sheetRange, bool isCaptain)
    {
        var players = new List<PlayerDataSourcePlayer>();

        var responsePlayers = _sheetsService.Service.Spreadsheets.Values
            .Get(sheetId, sheetRange).Execute()
            .Values ?? new List<IList<object>>();

        foreach (var row in responsePlayers.Where(x => x.Count > 3))
        {
            var playerId = row[7]?.ToString()?.Replace(@"https://www.dotabuff.com/players/", "");
            var playerName = row[0]?.ToString() ?? PlayerDataSourceConstants.DefaultPlayerName;

            if (long.TryParse(playerId, out var playerIdParsed))
            {
                players.Add(new PlayerDataSourcePlayer
                    { Id = playerIdParsed, Name = playerName, IsCaptain = isCaptain });
            }
        }

        return players;
    }

    private List<PlayerDataSourceTeam> GetTeamsFromSheet(string sheetId, string sheetRange,
        List<PlayerDataSourcePlayer> captains)
    {
        var teams = new List<PlayerDataSourceTeam>();

        var responsePlayers = _sheetsService.Service.Spreadsheets.Values
            .Get(sheetId, sheetRange).Execute()
            .Values ?? new List<IList<object>>();

        foreach (var captain in captains)
        {
            var team = new PlayerDataSourceTeam();
            team.Name = captain.Name;
            team.Players = new List<PlayerDataSourcePlayer>();
            team.Players.Add(new PlayerDataSourcePlayer { IsCaptain = true, Id = captain.Id, Name = captain.Name });

            var players = responsePlayers.Where(x => x[1].ToString() == captain.Name);

            foreach (var player in players)
            {
                var playerIdRaw = player[3].ToString()?.Replace(@"https://www.opendota.com/players/", "");
                var playerName = player[0].ToString() ?? PlayerDataSourceConstants.DefaultPlayerName;

                if (!long.TryParse(playerIdRaw, out var playerId))
                {
                    continue;
                }

                team.Players.Add(new PlayerDataSourcePlayer { Id = playerId, IsCaptain = false, Name = playerName });
            }

            teams.Add(team);
        }


        return teams;
    }
}