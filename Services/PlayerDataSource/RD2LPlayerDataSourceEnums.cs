namespace RD2LPowerRankings.Services.PlayerDataSource;

public enum PlayerColumns
{
    PickedBy = 0,
    PlayerName = 1,
    Blank = 2,
    Medal = 3,
    Rank = 4,
    OldMedal = 5,
    OldRank = 6,
    Value = 7,
    DotabuffUrl = 8,
    Pos1 = 9,
    Pos2 = 10,
    Pos3 = 11,
    Pos4 = 12,
    Pos5 = 13,
    Drafter = 14,
    TeamOrganizer = 15,
    Statement = 16
}

public enum PlayerColumnsSeason31
{
    EstimatedValue = 0,
    ZxyValue = 1,
    Cost = 2,
    Name = 3,
    DraftMMR = 4,
    Rank = 5,
    Screenshot = 6,
    IsVouched = 7,
    ActivityCheck = 8,
    Dotabuff = 9,
    OpenDota = 10,
    Captain = 11,
    JoinTimestamp = 12
}

public enum CaptainColumnsSeason31
{
    Name = 0,
    Rank = 1,
    DraftMMR = 2,
    Screenshot = 3,
    Statement = 4,
    IsVouched = 5,
    ActivityCheck = 6,
    Dotabuff = 7,
    Opendota = 8,
    JoinTimestamp = 9,
    Pos1 = 10,
    Pos2 = 11,
    Pos3 = 12,
    Pos4 = 13,
    Pos5 = 14,
    InGameDrafter = 15,
    TeamOrganizer = 16
}

public enum CaptainColumns
{
    Name = 0,
    Unused1 = 1,
    Medal = 2,
    Unused2 = 3,
    OldMedal = 4,
    Unused3 = 5,
    Value = 6,
    Statement = 4,
    DotabuffUrl = 12,
    OpenDotaUrl = 6,
    Pos1 = 7,
    Pos2 = 8,
    Pos3 = 9,
    Pos4 = 10,
    Pos5 = 11,
    Drafter = 12,
    TeamOrganizer = 13,
    DiscordId = 14
}

public enum PlayerColumnsSeason30
{
    Id = 0,
    CaptainName = 1,
    PlayerName = 2,
    Cost = 3,
    MMR = 4,
    PlayerId = 5,
    CaptainId = 6
}