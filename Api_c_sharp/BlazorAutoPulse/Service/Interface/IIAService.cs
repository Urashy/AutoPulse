using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IIAService
    {
        Task<ResultatAI> PredictAIAsync(DataAI data);
        Task<bool> HealthCheckAsync();
    }
}