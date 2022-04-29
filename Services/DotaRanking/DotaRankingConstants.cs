namespace RD2LPowerRankings.Modules.Dota;

public static class DotaRankingConstants
{
    public const decimal LobbyWeightSoloRanked = 1.0M;
    public const decimal LobbyWeightPartyRanked = 0.9M;
    public const decimal LobbyWeightSoloNormal = 0.8M;
    public const decimal LobbyWeightLeague = 1.2M;
    public const decimal LobbyWeightPartyNormal = 0.8M;
    public const decimal LobbyWeightBattleCup = 0.8M;

    public const int MaxGamesWeighted = 20;
    public const int MaxKDAWeighted = 20;

    public const decimal WinRateGamesPlayedFactor = 10M;
    public const decimal LobbyTypeFactor = 1M;
    public const decimal KDAFactor = 8M;
    public const decimal ToxicityAbandonFactor = 10000M;
    public const decimal ToxicityPingFactor = 25M;
    public const decimal ToxicityPingAbandonFactor = 1500M;
    public const decimal ToxicityWordFactor = 7M;

    public const int PowerRankRespectBansThreshold = 5;
    public const int PowerRankGamesPlayedThreshold = 1;
    public const decimal PowerRankWinRateThreshold = 0.35M;
    public const decimal WholesomeToxicityScoreThreshold = 125;
    public const int ExcessivePingThreshold = 50;

    public const decimal TeamSafelaneScoreFactor = 1.0M;
    public const decimal TeamMidlaneScoreFactor = 0.7M;
    public const decimal TeamOfflaneScoreFactor = 0.4M;
    public const decimal TeamSoftSupportScoreFactor = 0.2M;
    public const decimal TeamHardSupportScoreFactor = 0.1M;
}