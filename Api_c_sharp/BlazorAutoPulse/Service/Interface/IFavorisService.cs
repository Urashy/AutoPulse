using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFavorisService : IService<Favori>
    {
        Task<IEnumerable<Favori>> GetMesFavoris(int IdCompte);
        Task<bool> IsFavorite(int idCompte, int idAnnonce);
        Task<bool> ToggleFavorite(int idCompte, int idAnnonce);
    }
}