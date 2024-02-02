using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceLeague
{
    [DefaultValue("RD2L")] [Required] public string Name { get; set; } = null!;

    [DefaultValue("season-31")] [Required] public string FileName { get; set; } = null!;

    [Required] public List<PlayerDataSourceDivision> Divisions { get; set; } = new();

    [DefaultValue(15246)] public int? LeagueId { get; set; }
}