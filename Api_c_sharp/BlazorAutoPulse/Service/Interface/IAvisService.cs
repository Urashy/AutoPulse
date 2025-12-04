using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAvisService : IService<AvisListDTO>
    {
        Task<IEnumerable<AvisListDTO>> GetAvisByCompte(int id);
    }
}
