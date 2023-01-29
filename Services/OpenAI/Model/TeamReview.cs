using Newtonsoft.Json;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class TeamReview
{
    [JsonIgnore] public string Prompt { get; set; } = "";

    public string Result { get; set; } = "";
    [JsonIgnore] public string ReviewPrefixAdjective { get; set; } = "";
    [JsonIgnore] public List<string> Attributes { get; set; } = new();

    [JsonIgnore] public List<string> EndingSentences { get; set; } = new();
}