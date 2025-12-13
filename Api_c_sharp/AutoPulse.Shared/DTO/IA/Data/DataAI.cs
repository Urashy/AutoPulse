using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Data;

[JsonDerivedType(typeof(DataCNN), typeDiscriminator: "cnn")]
[JsonDerivedType(typeof(DataPrediction), typeDiscriminator: "prediction")]
[JsonDerivedType(typeof(DataAjustement), typeDiscriminator: "ajustement")]
public abstract class DataAI
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}