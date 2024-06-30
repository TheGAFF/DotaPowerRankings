using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1RBClnYnKyf5exH3N3Mt7J-R4z0cD4gBT4GnYbPvbLkA")]
    public string SheetId { get; set; } = null!;

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; } = null!;
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}