using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1NWVvW--B25ioSWj4FKL-z6ssiMqLY3oYXL07s3KGWiU")]
    public string SheetId { get; set; } = null!;

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; } = null!;
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}