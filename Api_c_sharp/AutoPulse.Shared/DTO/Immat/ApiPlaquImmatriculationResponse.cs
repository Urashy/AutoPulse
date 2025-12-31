namespace AutoPulse.Shared.DTO.Immat;

public class ApiPlaquImmatriculationResponse
{
    public ApiPlaquImmatriculationData? Data { get; set; }
        
    [System.Text.Json.Serialization.JsonPropertyName("api-version")]
    public string? ApiVersion { get; set; }
}