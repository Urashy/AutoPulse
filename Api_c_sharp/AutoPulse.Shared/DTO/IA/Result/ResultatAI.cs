using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public abstract class ResultatAI
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}