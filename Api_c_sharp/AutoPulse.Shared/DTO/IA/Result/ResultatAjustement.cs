using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public class ResultatAjustement : ResultatAI
{
    public override string Type => "ajustement";

    [JsonPropertyName("base_price")]
    public double BasePrice { get; set; }

    [JsonPropertyName("adjusted_price")]
    public double AdjustedPrice { get; set; }

    [JsonPropertyName("reduction_amount")]
    public double ReductionAmount { get; set; }

    [JsonPropertyName("reduction_percent")]
    public double ReductionPercent { get; set; }

    [JsonPropertyName("quality_coefficient")]
    public double QualityCoefficient { get; set; }

    [JsonPropertyName("category")]
    public string Category { get; set; }

    [JsonPropertyName("description_analyzed")]
    public string DescriptionAnalyzed { get; set; }
}