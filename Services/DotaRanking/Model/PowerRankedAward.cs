using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class PowerRankedAward
{
    public PowerRankedAward(string name, DotaEnums.AwardColor color)
    {
        Name = name;
        Color = color;
    }


    public string Name { get; set; }
    public DotaEnums.AwardColor Color { get; set; }
}