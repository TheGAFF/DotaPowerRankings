using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceLeague
{
    [DefaultValue("RD2L")] [Required] public string Name { get; set; } = null!;

    [DefaultValue("season-26")] [Required] public string FileName { get; set; } = null!;

    [Required] public List<PlayerDataSourceDivision> Divisions { get; set; } = new();


    [DefaultValue(13780)] public int? LeagueId { get; set; }
}