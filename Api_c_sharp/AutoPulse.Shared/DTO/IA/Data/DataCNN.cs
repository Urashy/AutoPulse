using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO.IA.Data;

public class DataCNN : DataAI
{
    public override string Type => "cnn";

    [JsonPropertyName("image_base64")]
    public string ImageBase64 { get; set; }
}