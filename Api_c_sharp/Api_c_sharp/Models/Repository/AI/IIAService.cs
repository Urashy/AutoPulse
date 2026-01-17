using AutoPulse.Shared.DTO.IA.Benchmark;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace Api_c_sharp.Models.Repository.AI;

public interface IIAService
{
    Task<ResultatAI> PredictAsync(DataAI data);
    Task<bool> HealthCheckAsync();
    Task<bool> ReloadModelsAsync();
    
    //Benchmark
    Task<IEnumerable<BenchmarkIAListDTO>> GetAllBenchmarksAsync();
    Task<BenchmarkIADTO?> GetBenchmarkByIdAsync(int id);
    Task<Dictionary<string, BenchmarkIADTO>> GetLatestBenchmarksByTypeAsync();
    Task<BenchmarkIAStatsDTO> GetBenchmarkStatsAsync();
    Task<IEnumerable<BenchmarkIAListDTO>> GetBenchmarkHistoryByTypeAsync(string modelType, int limit = 10);
    Task<BenchmarkIADTO> CreateBenchmarkAsync(BenchmarkIACreateDTO benchmark);
    Task<IEnumerable<BenchmarkIADTO>> SyncBenchmarksFromPythonAsync();
    Task<bool> DeleteBenchmarkAsync(int id);
}