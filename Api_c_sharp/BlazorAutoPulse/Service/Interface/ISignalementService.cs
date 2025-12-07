using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ISignalementService : IService<SignalementAnnonceCreateDTO>
{
    Task<IEnumerable<SignalementDTO>> GetAllAsync();
}