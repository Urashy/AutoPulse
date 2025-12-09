using System.Text.Json.Serialization;

namespace Api_c_sharp.Models.Entity;

public class GoogleTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }
}