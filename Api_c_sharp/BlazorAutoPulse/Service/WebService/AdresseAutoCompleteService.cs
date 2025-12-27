using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class AdresseAutoCompleteService : IAutoCompleteService
    {
        private readonly HttpClient _httpClient;

        public AdresseAutoCompleteService(HttpClient httpClient)
        {
            // ✅ Le HttpClient est déjà configuré via AddHttpClient dans Program.cs
            // Ne PAS modifier BaseAddress ou Headers ici
            _httpClient = httpClient;
        }

        public async Task<List<NominatimResult>> SearchAddressAsync(string query)
        {
            try
            {
                var url = $"search?q={Uri.EscapeDataString(query)}&format=json&addressdetails=1&limit=5&countrycodes=fr";
                Console.WriteLine($"🌐 URL complète: {_httpClient.BaseAddress}{url}");

                var response = await _httpClient.GetFromJsonAsync<List<NominatimResult>>(url);

                Console.WriteLine($"✅ API Nominatim: {response?.Count ?? 0} résultats");

                return response ?? new List<NominatimResult>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Erreur HTTP: {ex.StatusCode} - {ex.Message}");
                return new List<NominatimResult>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur recherche adresse: {ex.Message}");
                Console.WriteLine($"❌ Type: {ex.GetType().Name}");
                return new List<NominatimResult>();
            }
        }
    }
}