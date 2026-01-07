namespace AutoPulse.Shared.DTO.IA.Benchmark;

public class BenchmarkIAListDTO
{
    public int IdBenchmark { get; set; }
    public string ModelType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int TotalIterations { get; set; }
    public double SuccessRatePercent { get; set; }
    public double AvgInferenceTimeMs { get; set; }
    public double PredictionsPerSecond { get; set; }
}