using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IIAService
    {
        Task<ResultatAI> PredictAIAsync(DataAI data);
        Task<bool> HealthCheckAsync();
        
        //Benchmark
        Task<IEnumerable<BenchmarkIAListDTO>> GetAllBenchmarksAsync();
        Task<BenchmarkIADTO?> GetBenchmarkByIdAsync(int id);
        Task<Dictionary<string, BenchmarkIADTO>> GetLatestBenchmarksByTypeAsync();
        Task<BenchmarkIAStatsDTO> GetBenchmarkStatsAsync();
        Task<IEnumerable<BenchmarkIAListDTO>> GetBenchmarkHistoryByTypeAsync(string modelType, int limit = 10);
        Task<SyncBenchmarkResponseDTO> SyncBenchmarksFromPythonAsync();
        Task<bool> DeleteBenchmarkAsync(int id);
    }
}