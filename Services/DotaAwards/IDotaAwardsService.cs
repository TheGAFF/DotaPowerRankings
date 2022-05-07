using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaAwards;

public interface IDotaAwardsService
{
    public List<PowerRankedPlayer> GiveDivisionPlayerAwards(List<PowerRankedPlayer> players);

    public PowerRankedDivision GiveDivisionTeamAwards(PowerRankedDivision division);
}