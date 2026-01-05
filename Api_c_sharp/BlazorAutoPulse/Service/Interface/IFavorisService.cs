using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFavorisService : IService<FavoriDTO>
    {
        Task<IEnumerable<FavoriDTO>> GetMesFavoris(int IdCompte);
        Task<bool> IsFavorite(int idCompte, int idAnnonce);
        Task<bool> ToggleFavorite(int idCompte, int idAnnonce);
    }
}