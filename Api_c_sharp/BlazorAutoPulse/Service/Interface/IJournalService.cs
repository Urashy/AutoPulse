using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IJournalService : IService<JournalDTO>
    {
        Task<IEnumerable<JournalDTO>> GetFilteredAsync(RechercheJournalDTO rechercheDto);
    }
}