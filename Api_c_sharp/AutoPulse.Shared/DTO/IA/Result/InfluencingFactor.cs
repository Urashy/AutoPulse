using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public class InfluencingFactor
{
    [JsonPropertyName("feature")]
    public string Feature { get; set; }

    [JsonPropertyName("importance")]
    public double Importance { get; set; }
}