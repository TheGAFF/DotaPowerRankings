﻿using RD2LPowerRankings.Modules.Dota.Model;

namespace RD2LPowerRankings.Services.DotaRanking;

public interface IOpenAIService
{
    public Task<List<PowerRankedPlayer>> GeneratePlayerReviews(List<PowerRankedPlayer> players, string seasonName);

    Task<List<PowerRankedTeam>> GenerateTeamReviews(List<PowerRankedTeam> teams, string seasonName);
}