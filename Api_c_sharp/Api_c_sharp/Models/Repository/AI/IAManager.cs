using System.Text.Json;
using Api_c_sharp.Models.Entity;
using AutoMapper;
using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.AI;

public class IAManager : IIAService
{
    private readonly AutoPulseBdContext _context;
    private readonly IMapper _mapper;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IAManager> _logger;
    private readonly string _pythonApiUrl;

    public IAManager(AutoPulseBdContext context, IMapper mapper, HttpClient httpClient, IConfiguration configuration, ILogger<IAManager> logger)
    {
        _context = context;
        _mapper = mapper;
        _httpClient = httpClient;
        _logger = logger;
        _pythonApiUrl = configuration["PythonAPI:BaseUrl"] ?? "http://localhost:8000";
    }
    
    public virtual async Task<ResultatAI> PredictAsync(DataAI data)
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
    
    public virtual async Task<bool> HealthCheckAsync()
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

    public async Task<bool> ReloadModelsAsync()
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/reload", new {});
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public virtual async Task<IEnumerable<BenchmarkIAListDTO>> GetAllBenchmarksAsync()
    {
        var benchmarks = await _context.BenchmarksIA
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BenchmarkIAListDTO>>(benchmarks);
    }

    public virtual async Task<BenchmarkIADTO?> GetBenchmarkByIdAsync(int id)
    {
        var benchmark = await _context.BenchmarksIA
            .FirstOrDefaultAsync(b => b.IdBenchmark == id);

        return benchmark != null ? _mapper.Map<BenchmarkIADTO>(benchmark) : null;
    }

    public virtual async Task<Dictionary<string, BenchmarkIADTO>> GetLatestBenchmarksByTypeAsync()
    {
        var result = new Dictionary<string, BenchmarkIADTO>();
        var types = new[] { "cnn", "prediction", "ajustement" };

        foreach (var type in types)
        {
            var latest = await _context.BenchmarksIA
                .Where(b => b.ModelType == type)
                .OrderByDescending(b => b.Timestamp)
                .FirstOrDefaultAsync();

            if (latest != null)
            {
                result[type] = _mapper.Map<BenchmarkIADTO>(latest);
            }
        }

        return result;
    }

    public async Task<BenchmarkIAStatsDTO> GetBenchmarkStatsAsync()
    {
        var allBenchmarks = await _context.BenchmarksIA.ToListAsync();
        var latestByType = await GetLatestBenchmarksByTypeAsync();

        var stats = new BenchmarkIAStatsDTO
        {
            TotalBenchmarks = allBenchmarks.Count/3,
            BenchmarksByModel = allBenchmarks
                .GroupBy(b => b.ModelType)
                .ToDictionary(g => g.Key, g => g.Count()),
            GlobalAvgInferenceTimeMs = allBenchmarks.Any()
                ? allBenchmarks.Average(b => b.AvgInferenceTimeMs)
                : 0,
            GlobalSuccessRate = allBenchmarks.Any()
                ? allBenchmarks.Average(b => b.SuccessRatePercent)
                : 0,
            LatestCNN = latestByType.GetValueOrDefault("cnn"),
            LatestPrediction = latestByType.GetValueOrDefault("prediction"),
            LatestAjustement = latestByType.GetValueOrDefault("ajustement")
        };

        return stats;
    }

    public async Task<IEnumerable<BenchmarkIAListDTO>> GetBenchmarkHistoryByTypeAsync(string modelType, int limit = 10)
    {
        var benchmarks = await _context.BenchmarksIA
            .Where(b => b.ModelType == modelType)
            .OrderByDescending(b => b.Timestamp)
            .Take(limit)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BenchmarkIAListDTO>>(benchmarks);
    }

    public async Task<BenchmarkIADTO> CreateBenchmarkAsync(BenchmarkIACreateDTO benchmarkDto)
    {
        var benchmark = _mapper.Map<BenchmarkIA>(benchmarkDto);

        _context.BenchmarksIA.Add(benchmark);
        await _context.SaveChangesAsync();

        return _mapper.Map<BenchmarkIADTO>(benchmark);
    }

    public async Task<IEnumerable<BenchmarkIADTO>> SyncBenchmarksFromPythonAsync(
    IEnumerable<DataCNN> cnnData = null,
    IEnumerable<DataAjustement> ajustementData = null,
    IEnumerable<DataPrediction> predictionData = null)
    {
        try
        {
            _logger.LogInformation("Synchronisation des benchmarks depuis l'API Python");

            var createdBenchmarks = new List<BenchmarkIADTO>();

            if (cnnData != null && cnnData.Any())
            {
                _logger.LogInformation($"Lancement benchmark CNN avec {cnnData.Count()} images");
                
                var cnnRequest = new
                {
                    type = "cnn",
                    num_iterations = cnnData.Count(),
                    include_detailed_results = false,
                    test_images = cnnData.Select(d => d.ImageBase64).ToList()
                };

                var cnnResponse = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/benchmark", cnnRequest);
                cnnResponse.EnsureSuccessStatusCode();
                Console.WriteLine(cnnResponse.Content.ReadAsStringAsync().Result);

                var cnnResult = await cnnResponse.Content.ReadAsStringAsync();
                var cnnBenchmark = ParseBenchmarkFromJson(JsonSerializer.Deserialize<JsonElement>(cnnResult), "cnn");
                if (cnnBenchmark != null)
                {
                    var created = await CreateBenchmarkAsync(cnnBenchmark);
                    createdBenchmarks.Add(created);
                }
            }

            if (predictionData != null && predictionData.Any())
            {
                _logger.LogInformation($"Lancement benchmark Prix avec {predictionData.Count()} cas");
                
                var predictionRequest = new
                {
                    type = "prediction",
                    num_iterations = predictionData.Count(),
                    include_detailed_results = false,
                    test_cases = predictionData.Select(d => new
                    {
                        manufacturer = d.Manufacturer,
                        model = d.Model,
                        prod_year = d.ProdYear,
                        category = d.Category,
                        leather_interior = d.LeatherInterior,
                        fuel_type = d.FuelType,
                        engine_volume = d.EngineVolume,
                        mileage = d.Mileage,
                        cylinders = d.Cylinders,
                        gear_box_type = d.GearBoxType,
                        drive_wheels = d.DriveWheels,
                        doors = d.Doors,
                        wheel = d.Wheel,
                        color = d.Color,
                        airbags = d.Airbags
                    }).ToList()
                };

                var predictionResponse = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/benchmark", predictionRequest);
                predictionResponse.EnsureSuccessStatusCode();

                var predictionResult = await predictionResponse.Content.ReadAsStringAsync();
                var predictionBenchmark = ParseBenchmarkFromJson(JsonSerializer.Deserialize<JsonElement>(predictionResult), "prediction");
                if (predictionBenchmark != null)
                {
                    var created = await CreateBenchmarkAsync(predictionBenchmark);
                    createdBenchmarks.Add(created);
                }
            }
            
            if (ajustementData != null && ajustementData.Any())
            {
                _logger.LogInformation($"Lancement benchmark Ajustement avec {ajustementData.Count()} cas");
                
                var ajustementRequest = new
                {
                    type = "ajustement",
                    num_iterations = ajustementData.Count(),
                    include_detailed_results = false,
                    test_cases = ajustementData.Select(d => new
                    {
                        base_price = d.BasePrice,
                        description = d.Description
                    }).ToList()
                };

                var ajustementResponse = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/benchmark", ajustementRequest);
                ajustementResponse.EnsureSuccessStatusCode();

                var ajustementResult = await ajustementResponse.Content.ReadAsStringAsync();
                var ajustementBenchmark = ParseBenchmarkFromJson(JsonSerializer.Deserialize<JsonElement>(ajustementResult), "ajustement");
                if (ajustementBenchmark != null)
                {
                    var created = await CreateBenchmarkAsync(ajustementBenchmark);
                    createdBenchmarks.Add(created);
                }
            }

            _logger.LogInformation("Synchronisation terminée: {Count} benchmarks créés", createdBenchmarks.Count);
            return createdBenchmarks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la synchronisation des benchmarks");
            throw;
        }
    }

    public async Task<bool> DeleteBenchmarkAsync(int id)
    {
        var benchmark = await _context.BenchmarksIA.FindAsync(id);
        if (benchmark == null)
            return false;

        _context.BenchmarksIA.Remove(benchmark);
        await _context.SaveChangesAsync();
        return true;
    }

    private BenchmarkIACreateDTO? ParseBenchmarkFromJson(JsonElement element, string modelType)
    {
        try
        {
            _logger.LogInformation($"🔍 Parsing benchmark pour type: {modelType}");
            
            if (!element.TryGetProperty("stats", out var stats))
            {
                _logger.LogError("❌ Propriété 'stats' manquante");
                return null;
            }

            if (!element.TryGetProperty("system_info", out var systemInfo))
            {
                _logger.LogError("❌ Propriété 'system_info' manquante");
                return null;
            }

            var benchmark = new BenchmarkIACreateDTO
            {
                BenchmarkId = GetStringProperty(element, "benchmark_id", Guid.NewGuid().ToString()),
                ModelType = modelType,
                Timestamp = DateTime.SpecifyKind(
                    GetDateTimeProperty(element, "timestamp"),
                    DateTimeKind.Utc
                ),
                
                TotalIterations = GetIntProperty(stats, "total_iterations", 0),
                SuccessfulPredictions = GetIntProperty(stats, "successful_predictions", 0),
                FailedPredictions = GetIntProperty(stats, "failed_predictions", 0),
                SuccessRatePercent = GetDoubleProperty(stats, "success_rate_percent", 0.0),
                AvgInferenceTimeMs = GetDoubleProperty(stats, "avg_inference_time_ms", 0.0),
                MinInferenceTimeMs = GetDoubleProperty(stats, "min_inference_time_ms", 0.0),
                MaxInferenceTimeMs = GetDoubleProperty(stats, "max_inference_time_ms", 0.0),
                StdInferenceTimeMs = GetDoubleProperty(stats, "std_inference_time_ms", 0.0),
                PredictionsPerSecond = GetDoubleProperty(stats, "predictions_per_second", 0.0),
                TotalTimeSeconds = GetDoubleProperty(stats, "total_time_seconds", 0.0),
                
                Platform = GetStringProperty(systemInfo, "platform", "Unknown"),
                Processor = GetStringProperty(systemInfo, "processor", "Unknown"),
                PythonVersion = GetStringProperty(systemInfo, "python_version", "Unknown"),
                CpuCount = GetIntProperty(systemInfo, "cpu_count", 0),
                
                MemoryTotalGb = GetDoubleProperty(systemInfo, "system_memory_total_gb", 0.0),
                MemoryAvailableGb = GetDoubleProperty(systemInfo, "system_memory_available_gb", 0.0)
            };

            _logger.LogInformation($"✅ Benchmark parsé avec succès: {benchmark.BenchmarkId}");
            return benchmark;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors du parsing du benchmark {ModelType}", modelType);
            _logger.LogError($"JSON: {element.GetRawText()}");
            return null;
        }
    }

    // ============================================================================
    // MÉTHODES UTILITAIRES POUR LE PARSING SÉCURISÉ
    // ============================================================================

    private string GetStringProperty(JsonElement element, string propertyName, string defaultValue = "")
    {
        if (element.TryGetProperty(propertyName, out var property) && 
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? defaultValue;
        }
        _logger.LogDebug($"⚠️ Propriété '{propertyName}' non trouvée, utilisation de la valeur par défaut: '{defaultValue}'");
        return defaultValue;
    }

    private int GetIntProperty(JsonElement element, string propertyName, int defaultValue = 0)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetInt32();
            }
        }
        _logger.LogDebug($"⚠️ Propriété '{propertyName}' non trouvée, utilisation de la valeur par défaut: {defaultValue}");
        return defaultValue;
    }

    private double GetDoubleProperty(JsonElement element, string propertyName, double defaultValue = 0.0)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetDouble();
            }
        }
        _logger.LogDebug($"⚠️ Propriété '{propertyName}' non trouvée, utilisation de la valeur par défaut: {defaultValue}");
        return defaultValue;
    }

    private DateTime GetDateTimeProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) && 
            property.ValueKind == JsonValueKind.String)
        {
            var dateStr = property.GetString();
            if (DateTime.TryParse(dateStr, out var date))
            {
                return date;
            }
        }
        _logger.LogDebug($"⚠️ Propriété '{propertyName}' non trouvée, utilisation de DateTime.UtcNow");
        return DateTime.UtcNow;
    }
}