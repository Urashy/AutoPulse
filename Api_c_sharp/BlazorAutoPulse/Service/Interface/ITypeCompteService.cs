using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ITypeCompteService : IService<TypeCompteDTO>
    {
        Task<IEnumerable<TypeCompteDTO>> GetTypeComptesPourChercher();
    }
}
