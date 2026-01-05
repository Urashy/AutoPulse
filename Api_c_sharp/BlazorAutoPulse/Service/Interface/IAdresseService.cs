using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAdresseService : IService<AdresseDTO>
    {
        Task<IEnumerable<AdresseDTO>> GetAdresseByCompte(int id);
        Task<AdresseDTO> CreateAdresseAsync(AdresseCreateDTO entity);
        Task UpdateAdresseAsync(int id, AdresseUpdateDTO entity);
    }
}
