using System.Net.Http.Json;
using System.Text.Json;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class IAWebService : IIAService
    {
        private readonly HttpClient _httpClient;

        public IAWebService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ResultatAI> PredictAIAsync(DataAI data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/IA/Predict", data);
                response.EnsureSuccessStatusCode();
                
                var jsonString = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                options.Converters.Add(new ResultatAIJsonConverter());
        
                var result = JsonSerializer.Deserialize<ResultatAI>(jsonString, options);
                Console.WriteLine($"Résultat déserialize: {result?.GetType().Name}");
        
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR COMPLETE: {ex}");
                return new ResultatCNN { Success = false, Error = $"Erreur: {ex.Message}" };
            }
        }

        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/IA/Health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}