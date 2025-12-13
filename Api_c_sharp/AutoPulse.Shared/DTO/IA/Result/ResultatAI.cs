using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

[JsonDerivedType(typeof(ResultatCNN), typeDiscriminator: "cnn")]
[JsonDerivedType(typeof(ResultatPrediction), typeDiscriminator: "prediction")]
[JsonDerivedType(typeof(ResultatAjustement), typeDiscriminator: "ajustement")]
public abstract class ResultatAI
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}