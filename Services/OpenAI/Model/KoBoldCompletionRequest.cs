using Newtonsoft.Json;

namespace RD2LPowerRankings.Modules.Dota.Model;

public class KoBoldCompletionRequest
{
    [JsonProperty("max_context_length")] public int MaxContextLength { get; set; }

    [JsonProperty("max_length")] public int MaxLength { get; set; }

    [JsonProperty("prompt")] public string Prompt { get; set; }

    [JsonProperty("quiet")] public bool Quiet { get; set; }

    [JsonProperty("rep_pen")] public double RepPen { get; set; }

    [JsonProperty("rep_pen_range")] public int RepPenRange { get; set; }

    [JsonProperty("rep_pen_slope")] public int RepPenSlope { get; set; }

    [JsonProperty("temperature")] public double Temperature { get; set; }

    [JsonProperty("stop_sequence")] public string[] StopSequence { get; set; }

    [JsonProperty("tfs")] public int Tfs { get; set; }

    [JsonProperty("top_a")] public int TopA { get; set; }

    [JsonProperty("top_k")] public int TopK { get; set; }

    [JsonProperty("top_p")] public double TopP { get; set; }

    [JsonProperty("typical")] public double Typical { get; set; }
}