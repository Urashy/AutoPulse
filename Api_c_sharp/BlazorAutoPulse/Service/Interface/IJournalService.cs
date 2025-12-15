using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IJournalService : IService<JournalDTO>
    {
        Task<IEnumerable<JournalDTO>> GetFilteredAsync(int? typeId, DateTime? dateDebut, DateTime? dateFin, int ordre);
    }
}