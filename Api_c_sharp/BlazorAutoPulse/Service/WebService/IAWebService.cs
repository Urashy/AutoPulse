using System.Net.Http.Json;
using System.Text.Json;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class IAWebService : IIAService
    {
        private readonly HttpClient _httpClient;

        public IAWebService(IHttpClientFactory factory)
        {
            _httpClient = factory.CreateClient("ApiClient");
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
        
        public async Task<IEnumerable<BenchmarkIAListDTO>> GetAllBenchmarksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/BenchmarkIA/GetAll");
            response.EnsureSuccessStatusCode();

            var benchmarks = await response.Content.ReadFromJsonAsync<IEnumerable<BenchmarkIAListDTO>>();
            return benchmarks ?? new List<BenchmarkIAListDTO>();
        }
        catch (Exception ex)
        {
            return new List<BenchmarkIAListDTO>();
        }
    }

    public async Task<BenchmarkIADTO?> GetBenchmarkByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/BenchmarkIA/GetById/{id}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<BenchmarkIADTO>();
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<Dictionary<string, BenchmarkIADTO>> GetLatestBenchmarksByTypeAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/BenchmarkIA/GetLatestByType");
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, BenchmarkIADTO>>();
            return result ?? new Dictionary<string, BenchmarkIADTO>();
        }
        catch (Exception ex)
        {
            return new Dictionary<string, BenchmarkIADTO>();
        }
    }

    public async Task<BenchmarkIAStatsDTO> GetBenchmarkStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/BenchmarkIA/GetStats");
            response.EnsureSuccessStatusCode();

            var stats = await response.Content.ReadFromJsonAsync<BenchmarkIAStatsDTO>();
            return stats ?? new BenchmarkIAStatsDTO();
        }
        catch (Exception ex)
        {
            return new BenchmarkIAStatsDTO();
        }
    }

    public async Task<IEnumerable<BenchmarkIAListDTO>> GetBenchmarkHistoryByTypeAsync(string modelType, int limit = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/BenchmarkIA/GetHistoryByType/{modelType}?limit={limit}");
            response.EnsureSuccessStatusCode();

            var history = await response.Content.ReadFromJsonAsync<IEnumerable<BenchmarkIAListDTO>>();
            return history ?? new List<BenchmarkIAListDTO>();
        }
        catch (Exception ex)
        {
            return new List<BenchmarkIAListDTO>();
        }
    }

    public async Task<SyncBenchmarkResponseDTO> SyncBenchmarksFromPythonAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/BenchmarkIA/Sync", null);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<SyncBenchmarkResponseDTO>();
            
            return result ?? new SyncBenchmarkResponseDTO { Message = "Erreur de désérialisation" };
        }
        catch (HttpRequestException ex)
        {
            return new SyncBenchmarkResponseDTO 
            { 
                Message = "Service IA indisponible",
                Data = new List<BenchmarkIADTO>()
            };
        }
        catch (Exception ex)
        {
            return new SyncBenchmarkResponseDTO 
            { 
                Message = $"Erreur: {ex.Message}",
                Data = new List<BenchmarkIADTO>()
            };
        }
    }

    public async Task<bool> DeleteBenchmarkAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/BenchmarkIA/Delete/{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    }
}