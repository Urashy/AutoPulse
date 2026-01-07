namespace AutoPulse.Shared.DTO.IA.Benchmark;

public class BenchmarkIAStatsDTO
{
    public int TotalBenchmarks { get; set; }
    public Dictionary<string, int> BenchmarksByModel { get; set; } = new();
    public double GlobalAvgInferenceTimeMs { get; set; }
    public double GlobalSuccessRate { get; set; }
    public BenchmarkIADTO? LatestCNN { get; set; }
    public BenchmarkIADTO? LatestPrediction { get; set; }
    public BenchmarkIADTO? LatestAjustement { get; set; }
}