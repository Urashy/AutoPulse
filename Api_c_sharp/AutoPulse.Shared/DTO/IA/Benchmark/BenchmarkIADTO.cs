namespace AutoPulse.Shared.DTO.IA.Benchmark;

public class BenchmarkIADTO
{
    public int IdBenchmark { get; set; }
    public string BenchmarkId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int TotalIterations { get; set; }
    public int SuccessfulPredictions { get; set; }
    public int FailedPredictions { get; set; }
    public double SuccessRatePercent { get; set; }
    public double AvgInferenceTimeMs { get; set; }
    public double MinInferenceTimeMs { get; set; }
    public double MaxInferenceTimeMs { get; set; }
    public double StdInferenceTimeMs { get; set; }
    public double PredictionsPerSecond { get; set; }
    public double TotalTimeSeconds { get; set; }
    public string? Platform { get; set; }
    public string? Processor { get; set; }
    public string? PythonVersion { get; set; }
    public int? CpuCount { get; set; }
    public double? MemoryTotalGb { get; set; }
    public double? MemoryAvailableGb { get; set; }
}