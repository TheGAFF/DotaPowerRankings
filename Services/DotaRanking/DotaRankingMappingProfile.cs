using AutoMapper;
using RD2LPowerRankings.Database.Dota.Models;
using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaRanking;

public class DotaMappingProfile : Profile
{
    public DotaMappingProfile()
    {
        CreateMap<Player, PowerRankedPlayer>();
    }
}