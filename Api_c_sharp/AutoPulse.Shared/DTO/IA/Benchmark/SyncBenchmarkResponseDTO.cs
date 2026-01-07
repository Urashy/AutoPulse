namespace AutoPulse.Shared.DTO.IA.Benchmark;

public class SyncBenchmarkResponseDTO
{
    public string Message { get; set; } = string.Empty;
    public IEnumerable<BenchmarkIADTO> Data { get; set; } = new List<BenchmarkIADTO>();
}