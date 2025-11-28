using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFavorisService : IService<Favori>
    {
        Task<IEnumerable<Favori>> GetMesFavoris(int IdCompte);
    }
}
