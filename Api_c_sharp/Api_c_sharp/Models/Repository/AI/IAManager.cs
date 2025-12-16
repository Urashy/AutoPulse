using System.Text.Json;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace Api_c_sharp.Models.Repository.AI;

public class IAManager : IIAService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<IAManager> _logger;
    private readonly string _pythonApiUrl;

    public IAManager(HttpClient httpClient, IConfiguration configuration, ILogger<IAManager> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _pythonApiUrl = configuration["PythonAPI:BaseUrl"] ?? "http://localhost:8000";
    }

    /// <summary>
    /// Envoie une requête polymorphe à l'API Python et retourne le résultat correspondant
    /// </summary>
    /// <param name="data">Données à envoyer (CNN, Prediction ou Ajustement)</param>
    /// <returns>Résultat polymorphe correspondant au type de requête</returns>
    public async Task<ResultatAI> PredictAsync(DataAI data)
    {
        try
        {
            _logger.LogInformation("Envoi de la requête IA de type: {Type}", data.Type);

            // Construire la requête JSON avec le discriminateur de type
            var requestPayload = new
            {
                type = data.Type,
                data = data
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_pythonApiUrl}/predict",
                requestPayload
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Erreur API Python (HTTP {StatusCode}): {Error}",
                    response.StatusCode,
                    errorContent
                );
                throw new HttpRequestException(
                    $"L'API Python a retourné une erreur: {response.StatusCode}"
                );
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Réponse Python: {Response}", jsonResponse);

            // Désérialisation polymorphe basée sur le type
            ResultatAI? resultat = data.Type switch
            {
                "cnn" => JsonSerializer.Deserialize<ResultatCNN>(jsonResponse),
                "prediction" => JsonSerializer.Deserialize<ResultatPrediction>(jsonResponse),
                "ajustement" => JsonSerializer.Deserialize<ResultatAjustement>(jsonResponse),
                _ => throw new InvalidOperationException($"Type IA inconnu: {data.Type}")
            };

            if (resultat == null)
            {
                throw new JsonException("Échec de la désérialisation de la réponse Python");
            }

            _logger.LogInformation(
                "Prédiction réussie pour le type {Type}, Success={Success}",
                resultat.Type,
                resultat.Success
            );

            return resultat;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erreur de communication avec l'API Python");
            throw new Exception("Impossible de communiquer avec le service IA", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Erreur de parsing de la réponse JSON");
            throw new Exception("Format de réponse invalide du service IA", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors de l'appel IA");
            throw;
        }
    }

    /// <summary>
    /// Vérifie si l'API Python est accessible
    /// </summary>
    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_pythonApiUrl}/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}