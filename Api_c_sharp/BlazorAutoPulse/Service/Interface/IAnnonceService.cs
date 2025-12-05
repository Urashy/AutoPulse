using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAnnonceService : IService<Annonce>
    {
        Task<IEnumerable<AnnonceDTO>> GetByIdMiseEnAvant(int id, int pageNumber = 1, int pageSize = 21);
        Task<IEnumerable<AnnonceDTO>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams);
        Task<IEnumerable<AnnonceDTO>> GetByCompteID(int id);
        Task<IEnumerable<AnnonceDTO>> GetAnnoncesFavoritesByCompteId(int compteId);

    }

    public interface IAnnonceDetailService : IService<BlazorAutoPulse.Model.AnnonceDetailDTO>
    {
    }
}