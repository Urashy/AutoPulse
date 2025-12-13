using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;

namespace Api_c_sharp.Models.Repository.AI;

public interface IIAService
{
    Task<ResultatAI> PredictAsync(DataAI data);
    Task<bool> HealthCheckAsync();
}