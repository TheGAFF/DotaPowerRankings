namespace RD2LPowerRankings.Services.DotaDataSource;

public interface IDotaExtendedDataSource
{
    public Task LoadExtendedPlayerData(string sheetId);
}