using System.Net.Http.Json;
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

        public async Task<ResultatCNN> RecognizeVehicleAsync(DataCNN data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/IA/RecognizeVehicle",
                    data
                );

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ResultatCNN>()
                    ?? new ResultatCNN { Success = false, Error = "Réponse vide du serveur" };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erreur HTTP lors de la reconnaissance: {ex.Message}");
                return new ResultatCNN { Success = false, Error = $"Erreur de connexion: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la reconnaissance: {ex.Message}");
                return new ResultatCNN { Success = false, Error = $"Erreur: {ex.Message}" };
            }
        }

        public async Task<ResultatPrediction> PredictPriceAsync(DataPrediction data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/IA/PredictPrice",
                    data
                );

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ResultatPrediction>()
                    ?? new ResultatPrediction { Success = false, Error = "Réponse vide du serveur" };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erreur HTTP lors de la prédiction: {ex.Message}");
                return new ResultatPrediction { Success = false, Error = $"Erreur de connexion: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la prédiction: {ex.Message}");
                return new ResultatPrediction { Success = false, Error = $"Erreur: {ex.Message}" };
            }
        }

        public async Task<ResultatAjustement> AdjustPriceAsync(DataAjustement data)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"/api/IA/AdjustPrice",
                    data
                );

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ResultatAjustement>()
                    ?? new ResultatAjustement { Success = false, Error = "Réponse vide du serveur" };
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Erreur HTTP lors de l'ajustement: {ex.Message}");
                return new ResultatAjustement { Success = false, Error = $"Erreur de connexion: {ex.Message}" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajustement: {ex.Message}");
                return new ResultatAjustement { Success = false, Error = $"Erreur: {ex.Message}" };
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