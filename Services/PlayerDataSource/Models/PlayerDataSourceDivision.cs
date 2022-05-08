using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1Ag8ykOJwXRSh7Eq5k3MrMkgg7UGxaBSpYrydk1DjCxo")]
    public string SheetId { get; set; } = null!;

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; } = null!;
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}