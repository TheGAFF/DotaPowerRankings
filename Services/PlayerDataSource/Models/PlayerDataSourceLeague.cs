using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceLeague
{
    [DefaultValue("RD2L")] [Required] public string Name { get; set; }

    [DefaultValue("season-26")] [Required] public string FileName { get; set; }

    [Required] public List<PlayerDataSourceDivision> Divisions { get; set; }


    [DefaultValue(13780)] public int? LeagueId { get; set; }
}