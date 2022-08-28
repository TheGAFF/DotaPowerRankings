using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1P61G8dOFHfFR78CVHADrlkYwotLnDyoFer76fkQMHn8")]
    public string SheetId { get; set; } = null!;

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; } = null!;
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}