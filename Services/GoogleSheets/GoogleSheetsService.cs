using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace RD2LPowerRankings.Modules.GoogleSheets;

public class GoogleSheetsService : IGoogleSheetsService
{
    private readonly ILogger<GoogleSheetsService> _logger;
    private readonly string[] _readOnlyScopes = { SheetsService.Scope.SpreadsheetsReadonly };

    public GoogleSheetsService(ILogger<GoogleSheetsService> logger)
    {
        _logger = logger;
        InitializeService();
    }

    public SheetsService Service { get; set; } = default!;

    private void InitializeService()
    {
        var credential = GetCredentialsFromFile();
        Service = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = GoogleSheetsConstants.ApplicationName
        });
    }

    private GoogleCredential GetCredentialsFromFile()
    {
        GoogleCredential credential;
        using (var stream = new FileStream(GoogleSheetsConstants.SheetsKeyName, FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(_readOnlyScopes);
        }

        return credential;
    }
}