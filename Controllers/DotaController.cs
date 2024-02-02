using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RD2LPowerRankings.Modules.Dota;
using RD2LPowerRankings.Services.DotaDataSource;
using RD2LPowerRankings.Services.PlayerDataSource.Models;

namespace RD2LPowerRankings.Controllers;

[ApiController]
[Route("[controller]")]
public class DotaController : ControllerBase
{
    private readonly IDotaDataSource _dotaDataSource;
    private readonly IDotaExtendedDataSource _dotaExtendedDataSource;
    private readonly IDotaRankingService _dotaRankingService;
    private readonly ILogger<DotaController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public DotaController(ILogger<DotaController> logger, IDotaDataSource dotaDataSource,
        IDotaRankingService dotaRankingService, IWebHostEnvironment webHostEnvironment,
        IDotaExtendedDataSource dotaExtendedDataSource)
    {
        _logger = logger;
        _dotaDataSource = dotaDataSource;
        _dotaRankingService = dotaRankingService;
        _webHostEnvironment = webHostEnvironment;
        _dotaExtendedDataSource = dotaExtendedDataSource;
    }

    /// <summary>
    ///     Pulls players from sheet and loads their OpenDota data into the database.
    /// </summary>
    /// <param name="sheetId">The ID of the Google Sheet.</param>
    /// <returns></returns>
    [HttpGet(Name = "LoadPlayerData")]
    public async Task<bool> LoadPlayerData(
        [DefaultValue("1NWVvW--B25ioSWj4FKL-z6ssiMqLY3oYXL07s3KGWiU")]
        string sheetId)
    {
        _logger.LogInformation($"{nameof(LoadPlayerData)} Started");

        await _dotaDataSource.LoadPlayerData(sheetId);

        await _dotaExtendedDataSource.LoadExtendedPlayerData(sheetId);

        _logger.LogInformation($"{nameof(LoadPlayerData)} Finished");
        return true;
    }

    /// <summary>
    ///     Pulls players from the database and generates a json file with power rankings information.
    /// </summary>
    /// <param name="league"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    [ActionName("Post-Season-Rankings")]
    public async Task<bool> GetPowerRankings(PlayerDataSourceLeague league)
    {
        _logger.LogInformation($"{nameof(GetPowerRankings)} Started");

        using (var file = System.IO.File.CreateText($"{_webHostEnvironment.ContentRootPath}/{league.FileName}.json"))
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
                { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            serializer.Serialize(file, await _dotaRankingService.GeneratePostSeasonLeaguePowerRankings(league));
        }

        _logger.LogInformation($"{nameof(GetPowerRankings)} Finished");

        return true;
    }

    /// <summary>
    ///     Pulls players from the database and generates a json file with pre-season power rankings.
    /// </summary>
    /// <param name="league"></param>
    /// <returns></returns>
    [HttpPost("[action]")]
    [ActionName("Pre-Season-Rankings")]
    public async Task<bool> GetStartingPowerRankings(PlayerDataSourceLeague league)
    {
        _logger.LogInformation($"{nameof(GetPowerRankings)} Started");

        using (var file = System.IO.File.CreateText($"{_webHostEnvironment.ContentRootPath}/{league.FileName}.json"))
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
                { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            serializer.Serialize(file, await _dotaRankingService.GeneratePreSeasonLeaguePowerRankings(league));
        }

        _logger.LogInformation($"{nameof(GetPowerRankings)} Finished");

        return true;
    }
}