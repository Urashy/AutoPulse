using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ISignalementService : IService<SignalementCreateDTO>
{
    Task<IEnumerable<SignalementDTO>> GetAllAsync();
}