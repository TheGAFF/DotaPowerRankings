namespace RD2LPowerRankings.Services.Common;

public static class HttpRetryPolicies
{
    private const int MaxRetryAttempts = 4;

    public static TimeSpan[] GetBasicJitterRetryPolicy()
    {
        var retries = new List<TimeSpan>();

        for (var i = 0; i < MaxRetryAttempts; i++)
        {
            retries.Add(TimeSpan.FromSeconds(Math.Pow(3, i)) +
                        TimeSpan.FromMilliseconds(new Random().Next(0, 1000)));
        }

        return retries.ToArray();
    }
}