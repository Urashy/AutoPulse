using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ISignalementService : IService<SignalementAnnonceCreateDTO>
{
    Task<IEnumerable<SignalementDTO>> GetAllAsync();
    Task<bool> PostCompte(SignalementCreateDTO signalement);
    Task<bool> UpdateEtatAsync(int idSignalement, int nouvelEtat);

}