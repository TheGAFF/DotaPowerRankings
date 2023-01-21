using RD2LPowerRankings.Services.DotaRanking.Enums;

namespace RD2LPowerRankings.Modules.Dota;

public static class DotaRankingKeyValuePairs
{
    public static List<KeyValuePair<int, string>> ValidLeagues = new()
    {
        new KeyValuePair<int, string>(9872, "RD2L Mini Season 4"),
        new KeyValuePair<int, string>(10139, "RD2L Mini Season 5"),
        new KeyValuePair<int, string>(10457, "RD2L Mini Season 6"),
        new KeyValuePair<int, string>(9656, "RD2L Season 13"),
        new KeyValuePair<int, string>(9970, "RD2L Season 14"),
        new KeyValuePair<int, string>(10268, "RD2L Season 15"),
        new KeyValuePair<int, string>(10562, "RD2L Season 16"),
        new KeyValuePair<int, string>(10973, "RD2L Season 17"),
        new KeyValuePair<int, string>(11278, "RD2L Season 18"),
        new KeyValuePair<int, string>(11608, "RD2L Season 19"),
        new KeyValuePair<int, string>(11984, "RD2L Season 20"),
        new KeyValuePair<int, string>(12384, "RD2L Season 21"),
        new KeyValuePair<int, string>(12762, "RD2L Season 22"),
        new KeyValuePair<int, string>(13185, "RD2L Season 23"),
        new KeyValuePair<int, string>(13375, "RD2L Season 24"),
        new KeyValuePair<int, string>(13780, "RD2L Season 25"),
        new KeyValuePair<int, string>(14137, "RD2L Season 26"),
        new KeyValuePair<int, string>(14507, "RD2L Season 27"),

        new KeyValuePair<int, string>(5629, "RD2L Mini"),
        new KeyValuePair<int, string>(9858, "RD2L Weekend Cup"),
        new KeyValuePair<int, string>(12593, "RD2L Masters Season 1"),
        new KeyValuePair<int, string>(12950, "RD2L Masters Season 2"),
        new KeyValuePair<int, string>(13361, "RD2L Masters Season 3"),
        new KeyValuePair<int, string>(12939, "FHDL Season 1"),
        new KeyValuePair<int, string>(13679, "FHDL Season 3"),
        new KeyValuePair<int, string>(14078, "FHDL Season 4"),
        new KeyValuePair<int, string>(13941, "Phase Dota Circuit Season 1"),
        new KeyValuePair<int, string>(14107, "Phase Dota Circuit Season 2"),
        new KeyValuePair<int, string>(13807, "League of Lads Season 9"),
        new KeyValuePair<int, string>(13450, "League of Lads Season 8"),
        new KeyValuePair<int, string>(13177, "League of Lads Season 7"),

        new KeyValuePair<int, string>(14725, "Midwest Dota2 League Season 12")
    };

    public static readonly List<KeyValuePair<DotaEnums.Badge, decimal>> BadgeWeights =
        new()
        {
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Immortal, 650),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Divine5, 550),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Divine4, 525),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Divine3, 475),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Divine2, 450),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Divine1, 425),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Ancient5, 300),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Ancient4, 275),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Ancient3, 250),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Ancient2, 225),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Ancient1, 175),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Legend5, 150),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Legend4, 95),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Legend3, 90),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Legend2, 85),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Legend1, 80),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Archon5, 75),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Archon4, 70),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Archon3, 55),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Archon2, 50),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Archon1, 45),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Crusader5, 40),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Crusader4, 35),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Crusader3, 30),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Crusader2, 27),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Crusader1, 26),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Guardian5, 25),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Guardian4, 24),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Guardian3, 23),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Guardian2, 22),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Guardian1, 19),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Herald5, 18),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Herald4, 17),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Herald3, 16),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Herald2, 15),
            new KeyValuePair<DotaEnums.Badge, decimal>(DotaEnums.Badge.Herald1, 14)
        };
}