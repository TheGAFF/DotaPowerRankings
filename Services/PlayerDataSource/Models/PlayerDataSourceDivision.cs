using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1ieMbM7lfyIyom4mNbY33KwlRSu0yR2SdmAruCkIqG3I")]
    public string SheetId { get; set; } = null!;

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; } = null!;
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}