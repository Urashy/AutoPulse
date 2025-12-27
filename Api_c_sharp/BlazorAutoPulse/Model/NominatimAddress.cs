using System.Text.Json.Serialization;

namespace BlazorAutoPulse.Model
{
    public class NominatimAddress
    {
        [JsonPropertyName("house_number")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("road")]
        public string Road { get; set; }

        [JsonPropertyName("suburb")]
        public string Suburb { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("town")]
        public string Town { get; set; }

        [JsonPropertyName("village")]
        public string Village { get; set; }

        [JsonPropertyName("postcode")]
        public string Postcode { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        public string GetCity()
        {
            return City ?? Town ?? Village ?? Suburb ?? "";
        }
    }
}
