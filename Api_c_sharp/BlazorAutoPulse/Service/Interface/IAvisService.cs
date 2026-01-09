using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAvisService : IService<AvisListDTO>
    {
        Task<IEnumerable<AvisListDTO>> GetAvisByCompte(int id);
        Task<ServiceResult<AvisListDTO>> CreateAvis(AvisCreateDTO avis);
    }
}
