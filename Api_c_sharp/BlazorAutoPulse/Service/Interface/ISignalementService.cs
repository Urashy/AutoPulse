using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ISignalementService : IService<SignalementCreateDTO>
{
    Task<IEnumerable<SignalementDTO>> GetAllSignalementsAsync();
    Task UpdateSignalementAsync(int id, SignalementUpdateDTO entity);
    Task<IEnumerable<SignalementDTO>> GetFiltered(int idetat, int idtype, string recherche);

}