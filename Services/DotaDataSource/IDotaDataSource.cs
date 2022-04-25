namespace RD2LPowerRankings.Services.DotaDataSource;

public interface IDotaDataSource
{
    public Task LoadPlayerData(string sheetId);
}