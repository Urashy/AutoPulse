using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public class ResultatPrediction : ResultatAI
{
    public override string Type => "prediction";

    [JsonPropertyName("predicted_price")]
    public double PredictedPrice { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; }

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("top_influencing_factors")]
    public List<InfluencingFactor> TopInfluencingFactors { get; set; }
}