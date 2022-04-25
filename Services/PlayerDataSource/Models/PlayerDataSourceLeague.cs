using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceLeague
{
    [DefaultValue("RD2L")] [Required] public string Name { get; set; }

    [Required] public List<PlayerDataSourceDivision> Divisions { get; set; }
}