using AutoMapper;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Modules.Dota;

public class DotaMappingProfile : Profile
{
    public DotaMappingProfile()
    {
        CreateMap<Database.Dota.Models.Player, PowerRankedPlayer>();
    }
}