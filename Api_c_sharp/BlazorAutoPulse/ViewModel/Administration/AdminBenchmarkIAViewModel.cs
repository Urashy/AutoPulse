using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Benchmark;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration;

public class AdminBenchmarkIAViewModel
{
    private readonly IIAService _iaService;
    private Action? _refreshUI;

    public bool IsLoading { get; private set; } = true;
    public bool IsSyncing { get; private set; } = false;
    public bool IsHealthy { get; private set; } = false;
    public bool IsCheckingHealth { get; private set; } = false;

    // Derniers benchmarks par type
    public BenchmarkIADTO? LatestCNN { get; private set; }
    public BenchmarkIADTO? LatestPrediction { get; private set; }
    public BenchmarkIADTO? LatestAjustement { get; private set; }

    // Historique
    public List<BenchmarkIAListDTO> HistoryList { get; private set; } = new();
    public string SelectedHistoryType { get; private set; } = "all";
    public int HistoryLimit { get; private set; } = 10;

    // Statistiques globales
    public BenchmarkIAStatsDTO? GlobalStats { get; private set; }

    // Messages
    public string? StatusMessage { get; private set; }
    public bool ShowSuccessMessage { get; private set; }
    public bool ShowErrorMessage { get; private set; }

    public AdminBenchmarkIAViewModel(IIAService iaService)
    {
        _iaService = iaService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
        await LoadAllData();
    }

    private async Task LoadAllData()
    {
        IsLoading = true;
        _refreshUI?.Invoke();

        try
        {
            // Charger les derniers benchmarks
            await LoadLatestBenchmarks();

            // Charger les statistiques
            await LoadStats();

            // Charger l'historique
            await LoadHistory();

            // Vérifier la santé de l'API IA
            await CheckIAHealth();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement des données: {ex.Message}");
            ShowError("Erreur lors du chargement des données");
        }
        finally
        {
            IsLoading = false;
            _refreshUI?.Invoke();
        }
    }

    private async Task LoadLatestBenchmarks()
    {
        try
        {
            var latest = await _iaService.GetLatestBenchmarksByTypeAsync();
            
            LatestCNN = latest["cnn"];
            LatestPrediction = latest["prediction"];
            LatestAjustement = latest["ajustement"];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LoadLatestBenchmarks: {ex.Message}");
        }
    }

    private async Task LoadStats()
    {
        try
        {
            GlobalStats = await _iaService.GetBenchmarkStatsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LoadStats: {ex.Message}");
        }
    }

    private async Task LoadHistory()
    {
        try
        {
            if (SelectedHistoryType == "all")
            {
                var allHistory = await _iaService.GetAllBenchmarksAsync();
                HistoryList = allHistory.Take(HistoryLimit).ToList();
            }
            else
            {
                var typeHistory = await _iaService.GetBenchmarkHistoryByTypeAsync(
                    SelectedHistoryType, 
                    HistoryLimit
                );
                HistoryList = typeHistory.ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur LoadHistory: {ex.Message}");
            HistoryList = new List<BenchmarkIAListDTO>();
        }
    }

    public async Task CheckIAHealth()
    {
        IsCheckingHealth = true;
        _refreshUI?.Invoke();

        try
        {
            IsHealthy = await _iaService.HealthCheckAsync();
            
            if (IsHealthy)
            {
                ShowSuccess("Service IA opérationnel ✓");
            }
            else
            {
                ShowError("Service IA indisponible");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur CheckIAHealth: {ex.Message}");
            IsHealthy = false;
            ShowError("Impossible de vérifier le service IA");
        }
        finally
        {
            IsCheckingHealth = false;
            _refreshUI?.Invoke();
        }
    }

    public async Task SyncBenchmarks()
    {
        IsSyncing = true;
        _refreshUI?.Invoke();

        try
        {
            var result = await _iaService.SyncBenchmarksFromPythonAsync();
            
            if (result.Data.Any())
            {
                ShowSuccess($"{result.Data.Count()} benchmark(s) synchronisé(s) avec succès");
                await LoadAllData();
            }
            else
            {
                ShowError(result.Message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur SyncBenchmarks: {ex.Message}");
            ShowError("Erreur lors de la synchronisation");
        }
        finally
        {
            IsSyncing = false;
            _refreshUI?.Invoke();
        }
    }

    public async Task FilterHistoryByType(string type)
    {
        if (SelectedHistoryType != type)
        {
            SelectedHistoryType = type;
            await LoadHistory();
            _refreshUI?.Invoke();
        }
    }

    public async Task ChangeHistoryLimit(int limit)
    {
        if (HistoryLimit != limit)
        {
            HistoryLimit = limit;
            await LoadHistory();
            _refreshUI?.Invoke();
        }
    }

    public async Task RefreshData()
    {
        await LoadAllData();
    }

    private void ShowSuccess(string message)
    {
        StatusMessage = message;
        ShowSuccessMessage = true;
        ShowErrorMessage = false;
        
        // Auto-hide après 3 secondes
        Task.Delay(3000).ContinueWith(_ =>
        {
            ShowSuccessMessage = false;
            _refreshUI?.Invoke();
        });
    }

    private void ShowError(string message)
    {
        StatusMessage = message;
        ShowErrorMessage = true;
        ShowSuccessMessage = false;
        
        // Auto-hide après 5 secondes
        Task.Delay(5000).ContinueWith(_ =>
        {
            ShowErrorMessage = false;
            _refreshUI?.Invoke();
        });
    }

    // Helper methods pour l'UI
    public string GetHealthStatusClass() => IsHealthy ? "status-healthy" : "status-unhealthy";
    public string GetHealthStatusText() => IsHealthy ? "Opérationnel" : "Hors ligne";
    public string GetHealthStatusIcon() => IsHealthy ? "✓" : "✗";

    public string GetModelTypeDisplayName(string type) => type switch
    {
        "cnn" => "Reconnaissance Visuelle (CNN)",
        "prediction" => "Prédiction de Prix",
        "ajustement" => "Ajustement de Prix",
        _ => type
    };

    public string GetModelTypeIcon(string type) => type switch
    {
        "cnn" => "🖼️",
        "prediction" => "💰",
        "ajustement" => "⚖️",
        _ => "🤖"
    };

    public string GetPerformanceLevel(double avgInferenceMs) => avgInferenceMs switch
    {
        < 10 => "Excellent",
        < 50 => "Très bon",
        < 100 => "Bon",
        < 200 => "Moyen",
        _ => "À améliorer"
    };

    public string GetPerformanceClass(double avgInferenceMs) => avgInferenceMs switch
    {
        < 10 => "perf-excellent",
        < 50 => "perf-good",
        < 100 => "perf-medium",
        < 200 => "perf-low",
        _ => "perf-poor"
    };
}