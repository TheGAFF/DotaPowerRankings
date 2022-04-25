using Google.Apis.Sheets.v4;

namespace RD2LPowerRankings.Modules.GoogleSheets;

public interface IGoogleSheetsService
{
    public SheetsService Service { get; set; }
}