using System.Text.Json;
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
    
    public async Task<IEnumerable<BenchmarkIAListDTO>> GetAllBenchmarksAsync()
    {
        var benchmarks = await _context.BenchmarksIA
            .OrderByDescending(b => b.Timestamp)
            .ToListAsync();

        return _mapper.Map<IEnumerable<BenchmarkIAListDTO>>(benchmarks);
    }

    public async Task<BenchmarkIADTO?> GetBenchmarkByIdAsync(int id)
    {
        var benchmark = await _context.BenchmarksIA
            .FirstOrDefaultAsync(b => b.IdBenchmark == id);

        return benchmark != null ? _mapper.Map<BenchmarkIADTO>(benchmark) : null;
    }

    public async Task<Dictionary<string, BenchmarkIADTO>> GetLatestBenchmarksByTypeAsync()
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
            TotalBenchmarks = allBenchmarks.Count,
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
        var benchmark = _mapper.Map<Entity.BenchmarkIA>(benchmarkDto);

        _context.BenchmarksIA.Add(benchmark);
        await _context.SaveChangesAsync();

        return _mapper.Map<BenchmarkIADTO>(benchmark);
    }

    public async Task<IEnumerable<BenchmarkIADTO>> SyncBenchmarksFromPythonAsync()
    {
        try
        {
            _logger.LogInformation("Synchronisation des benchmarks depuis l'API Python");

            var response = await _httpClient.PostAsJsonAsync($"{_pythonApiUrl}/benchmark/all", new {});
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var benchmarkData = JsonSerializer.Deserialize<JsonElement>(jsonString);

            var createdBenchmarks = new List<BenchmarkIADTO>();

            // Traiter chaque type de benchmark
            var types = new[] { "cnn_benchmark", "price_benchmark", "adjustment_benchmark" };
            var modelTypes = new[] { "cnn", "prediction", "ajustement" };

            for (int i = 0; i < types.Length; i++)
            {
                if (benchmarkData.TryGetProperty(types[i], out var benchmarkElement))
                {
                    var benchmark = ParseBenchmarkFromJson(benchmarkElement, modelTypes[i]);
                    if (benchmark != null)
                    {
                        var created = await CreateBenchmarkAsync(benchmark);
                        createdBenchmarks.Add(created);
                    }
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
            var stats = element.GetProperty("stats");
            var systemInfo = element.GetProperty("system_info");

            return new BenchmarkIACreateDTO()
            {
                BenchmarkId = element.GetProperty("benchmark_id").GetString() ?? string.Empty,
                ModelType = modelType,
                Timestamp = DateTime.SpecifyKind(
                    DateTime.Parse(
                        element.GetProperty("timestamp").GetString() 
                        ?? DateTime.UtcNow.ToString()
                    ),
                    DateTimeKind.Utc
                ),
                TotalIterations = stats.GetProperty("total_iterations").GetInt32(),
                SuccessfulPredictions = stats.GetProperty("successful_predictions").GetInt32(),
                FailedPredictions = stats.GetProperty("failed_predictions").GetInt32(),
                SuccessRatePercent = stats.GetProperty("success_rate_percent").GetDouble(),
                AvgInferenceTimeMs = stats.GetProperty("avg_inference_time_ms").GetDouble(),
                MinInferenceTimeMs = stats.GetProperty("min_inference_time_ms").GetDouble(),
                MaxInferenceTimeMs = stats.GetProperty("max_inference_time_ms").GetDouble(),
                StdInferenceTimeMs = stats.GetProperty("std_inference_time_ms").GetDouble(),
                PredictionsPerSecond = stats.GetProperty("predictions_per_second").GetDouble(),
                TotalTimeSeconds = stats.GetProperty("total_time_seconds").GetDouble(),
                Platform = systemInfo.GetProperty("platform").GetString(),
                Processor = systemInfo.GetProperty("processor").GetString(),
                PythonVersion = systemInfo.GetProperty("python_version").GetString(),
                CpuCount = systemInfo.GetProperty("cpu_count").GetInt32(),
                MemoryTotalGb = systemInfo.GetProperty("memory_total_gb").GetDouble(),
                MemoryAvailableGb = systemInfo.GetProperty("memory_available_gb").GetDouble()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du parsing du benchmark {ModelType}", modelType);
            return null;
        }
    }
}