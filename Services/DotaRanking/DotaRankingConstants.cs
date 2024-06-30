namespace RD2LPowerRankings.Services.DotaRanking;

public static class DotaRankingConstants
{
    public const int MaxGamesPlayed = 100;
    public const int MaxBadge = 80;
    public const int MinBadge = 11;
    public const decimal MaxWinRate = 1.0M;
    public const int MaxLeaderboardRank = 5000;
    public const int MinLeaderboardRank = 1;
    public const int MinImpact = -100;
    public const int MaxImpact = 100;

    public const decimal WeightWinRate = 2.0M;
    public const decimal WeightGamesPlayed = 1.5M;
    public const decimal WeightBadge = 8M;
    public const decimal WeightLeaderboard = 4M;
    public const decimal WeightImpact = 3M;

    public const decimal LobbyWeightSoloRanked = 1.0M;
    public const decimal LobbyWeightPartyRanked = 0.9M;
    public const decimal LobbyWeightSoloNormal = 0.8M;
    public const decimal LobbyWeightLeague = 1.4M;
    public const decimal LobbyWeightPartyNormal = 0.8M;
    public const decimal LobbyWeightBattleCup = 0.8M;

    public const decimal ToxicityAbandonFactor = 10000M;
    public const decimal ToxicityPingFactor = 25M;
    public const decimal ToxicityPingAbandonFactor = 1500M;
    public const decimal ToxicityWordFactor = 7M;
    public const decimal ToxicityIntentionalFeedingFactor = 250000M;

    public const int PowerRankRespectBansThreshold = 5;
    public const int PowerRankGamesPlayedThreshold = 1;
    public const decimal PowerRankWinRateThreshold = 0.35M;
    public const decimal WholesomeToxicityScoreThreshold = 125;
    public const int ExcessivePingThreshold = 50;

    public const decimal TeamSafelaneScoreFactor = 1.0M;
    public const decimal TeamMidlaneScoreFactor = 0.9M;
    public const decimal TeamOfflaneScoreFactor = 0.7M;
    public const decimal TeamSoftSupportScoreFactor = 0.5M;
    public const decimal TeamHardSupportScoreFactor = 0.4M;
}