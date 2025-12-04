using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAdresseService : IService<Adresse>
    {
        Task<IEnumerable<AdresseDTO>> GetAdresseByCompte(int id);
    }
}
