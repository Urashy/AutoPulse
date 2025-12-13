using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Data;

public class DataAjustement : DataAI
{
    public override string Type => "ajustement";

    [JsonPropertyName("base_price")]
    public double BasePrice { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }
}