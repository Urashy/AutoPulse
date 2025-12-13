using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public class TopPrediction
{
    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; }
}