using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RD2LPowerRankings.Services.PlayerDataSource.Models;

public class PlayerDataSourceDivision
{
    [Required]
    [DefaultValue("1jf72KRLmKw93VfGoKBy0FdBIwCgiMY4DmqpynrP-mb8")]
    public string SheetId { get; set; }

    [Required] [DefaultValue("EST-TUE")] public string Name { get; set; }
    [JsonIgnore] [DefaultValue(null)] public List<PlayerDataSourceTeam>? Teams { get; set; }
}