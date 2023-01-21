using Newtonsoft.Json;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class CompletionRequest
{
    [JsonProperty("model")] public string Model { get; set; }

    [JsonProperty("prompt")] public string Prompt { get; set; }

    [JsonProperty("max_tokens")] public int MaxTokens { get; set; }

    [JsonProperty("temperature")] public double? Temperature { get; set; }
}