using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ISignalementService : IService<SignalementCreateDTO>
{
    Task<IEnumerable<SignalementDTO>> GetAllSignalementsAsync();
    Task<bool> UpdateEtatAsync(int idSignalement, int nouvelEtat);
    Task<IEnumerable<SignalementDTO>> GetFiltered(int idetat, int idtype, string recherche);

}