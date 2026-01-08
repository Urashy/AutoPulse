using AutoPulse.Shared.DTO.IA.Benchmark;

namespace BlazorAutoPulse.ViewModel;

public class BenchmarkCardComponentViewModel
{
    public BenchmarkIADTO Benchmark { get; set; } = null!;
    public string ModelName { get; set; }
    public string ModelIcon { get; set; }

    public async Task InitializeAsync(BenchmarkIADTO _benchmark)
    {
        Benchmark = _benchmark;
        ModelName = GetModelName();
        ModelIcon = GetModelIcon();
    }
    
    public string GetModelName()
    {
        return Benchmark.ModelType switch
        {
            "cnn" => "Reconnaissance Visuelle",
            "prediction" => "Prédiction de Prix",
            "ajustement" => "Ajustement de Prix",
            _ => "Modèle IA"
        };
    }

    public string GetModelIcon()
    {
        return Benchmark.ModelType switch
        {
            "cnn" => "🖼️",
            "prediction" => "💰",
            "ajustement" => "⚖️",
            _ => "🤖"
        };
    }

    public string GetPerformanceLevel()
    {
        return Benchmark.AvgInferenceTimeMs switch
        {
            < 10 => "Excellent",
            < 50 => "Très bon",
            < 100 => "Bon",
            < 200 => "Moyen",
            _ => "À améliorer"
        };
    }

    public string GetPerformanceClass()
    {
        return Benchmark.AvgInferenceTimeMs switch
        {
            < 10 => "perf-excellent",
            < 50 => "perf-good",
            < 100 => "perf-medium",
            < 200 => "perf-low",
            _ => "perf-poor"
        };
    }

    public int GetAvgPosition()
    {
        if (Benchmark.MaxInferenceTimeMs == Benchmark.MinInferenceTimeMs)
            return 50;

        var range = Benchmark.MaxInferenceTimeMs - Benchmark.MinInferenceTimeMs;
        var avgOffset = Benchmark.AvgInferenceTimeMs - Benchmark.MinInferenceTimeMs;
        return (int)((avgOffset / range) * 100);
    }
}