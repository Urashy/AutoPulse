using System.Text.Json.Serialization;

namespace AutoPulse.Shared.DTO;

public class OffreNotification
{
    [JsonPropertyName("idOffre")]
    public int IdOffre { get; set; }
        
    [JsonPropertyName("valeur")]
    public decimal Valeur { get; set; }
        
    [JsonPropertyName("annonceLibelle")]
    public string AnnonceLibelle { get; set; } = string.Empty;
        
    [JsonPropertyName("receivedAt")]
    public DateTime ReceivedAt { get; set; }
}