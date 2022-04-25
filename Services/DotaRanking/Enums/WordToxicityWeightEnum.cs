namespace RD2LPowerRankings.Services.DotaRanking.Enums;

public partial class DotaEnums
{
    public enum WordToxicity
    {
        SlightlyRacist = 100,
        ModeratelyRacist = 200,
        Racist = 1000,
        RacistFalsePositive = -1000,
        PoliticallyIncorrect = 100,
        Homophobic = 200,
        VeryHomophobic = 1000,
        Sexist = 50,
        NotNice = 5,
        NotNiceAtAll = 10,
        SlightlyToxic = 25,
        Toxic = 50,
        VeryToxic = 200,
        Wholesome = -1,
        VeryWholesome = -2
    }
}