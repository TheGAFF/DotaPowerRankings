using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Services.DotaDataSource;

public static class DotaDataHelpers
{
    public static DotaEnums.Lane LaneStringToEnum(string lane)
    {
        switch (lane)
        {
            case "SAFE_LANE":
                return DotaEnums.Lane.Safe;
            case "MID_LANE":
                return DotaEnums.Lane.Mid;
            case "OFF_LANE":
                return DotaEnums.Lane.Off;
            default:
                return DotaEnums.Lane.Off;
        }
    }

    public static DotaEnums.TeamRole RoleStringToEnum(string role, DotaEnums.Lane lane)
    {
        if (role == "LIGHT_SUPPORT")
        {
            return DotaEnums.TeamRole.SoftSupport;
        }

        if (role == "HARD_SUPPORT")
        {
            return DotaEnums.TeamRole.HardSupport;
        }

        if (lane == DotaEnums.Lane.Off)
        {
            return DotaEnums.TeamRole.Offlane;
        }

        if (lane == DotaEnums.Lane.Safe)
        {
            return DotaEnums.TeamRole.Safelane;
        }

        return DotaEnums.TeamRole.Midlane;
    }


    public static DotaEnums.LobbyType LobbyTypeTextToEnum(string lobby)
    {
        return lobby switch
        {
            "UNRANKED" => DotaEnums.LobbyType.Normal,
            "PRACTICE" => DotaEnums.LobbyType.Practice,
            "TUTORIAL" => DotaEnums.LobbyType.Tutorial,
            "COOP_VS_BOTS" => DotaEnums.LobbyType.CooperativeBots,
            "TEAM_MATCH" => DotaEnums.LobbyType.RankedTeamMM,
            "SOLO_QUEUE" => DotaEnums.LobbyType.RankedSoloMM,
            "RANKED" => DotaEnums.LobbyType.Ranked,
            "SOLO_MID" => DotaEnums.LobbyType.OneVsOneMid,
            "BATTLE_CUP" => DotaEnums.LobbyType.BattleCup,
            _ => DotaEnums.LobbyType.Unknown
        };
    }

    public static DotaEnums.GameMode GameModeTextToEnum(string gameMode)
    {
        return gameMode switch
        {
            "NONE" => DotaEnums.GameMode.Unknown,
            "ALL_PICK" => DotaEnums.GameMode.AllPick,
            "CAPTAINS_MODE" => DotaEnums.GameMode.CaptainsMode,
            "RANDOM_DRAFT" => DotaEnums.GameMode.RandomDraft,
            "SINGLE_DRAFT" => DotaEnums.GameMode.SingleDraft,
            "ALL_RANDOM" => DotaEnums.GameMode.AllRandom,
            "INTRO" => DotaEnums.GameMode.Intro,
            "THE_DIRETIDE" => DotaEnums.GameMode.Diretide,
            "REVERSE_CAPTAINS_MODE" => DotaEnums.GameMode.ReverseCaptainsMode,
            "THE_GREEVILING" => DotaEnums.GameMode.TheGreeviling,
            "TUTORIAL" => DotaEnums.GameMode.Tutorial,
            "MID_ONLY" => DotaEnums.GameMode.MidOnly,
            "LEAST_PLAYED" => DotaEnums.GameMode.LeastPlayed,
            "NEW_PLAYER_POOL" => DotaEnums.GameMode.Unknown,
            "COMPENDIUM_MATCHMAKING" => DotaEnums.GameMode.Compendium,
            "CUSTOM" => DotaEnums.GameMode.Custom,
            "CAPTAINS_DRAFT" => DotaEnums.GameMode.CaptainsDraft,
            "BALANCED_DRAFT" => DotaEnums.GameMode.BalancedDraft,
            "ABILITY_DRAFT" => DotaEnums.GameMode.AbilityDraft,
            "EVENT" => DotaEnums.GameMode.Event,
            "ALL_RANDOM_DEATH_MATCH" => DotaEnums.GameMode.AllRandomDeathMatch,
            "SOLO_MID" => DotaEnums.GameMode.OneVsOneSoloMid,
            "ALL_PICK_RANKED" => DotaEnums.GameMode.AllDraft,
            "TURBO" => DotaEnums.GameMode.Turbo,
            "MUTATION" => DotaEnums.GameMode.Unknown,
            _ => DotaEnums.GameMode.Unknown
        };
    }
}