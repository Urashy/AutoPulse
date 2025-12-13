using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Result;

public class ResultatCNN : ResultatAI
{
    public override string Type => "cnn";

    [JsonPropertyName("manufacturer")]
    public string Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; }

    [JsonPropertyName("confidence")]
    public string Confidence { get; set; }

    [JsonPropertyName("confidence_score")]
    public double ConfidenceScore { get; set; }

    [JsonPropertyName("image_size")]
    public string ImageSize { get; set; }

    [JsonPropertyName("top_predictions")]
    public List<TopPrediction> TopPredictions { get; set; }
}