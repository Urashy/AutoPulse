using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using AnnonceDetailDTO = AutoPulse.Shared.DTO.AnnonceDetailDTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAnnonceService : IService<AnnonceDTO>
    {
        Task<IEnumerable<AnnonceDTO>> GetByIdMiseEnAvant(int id, int pageNumber = 1, int pageSize = 21);
        Task<AnnonceDTO> CreateAnnonceAsync(AnnonceCreateDTO entity);
        Task UpdateAnnonceAsync(int id, AnnonceCreateDTO entity);
        Task<IEnumerable<AnnonceDTO>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams);
        Task<IEnumerable<AnnonceDTO>> GetByCompteID(int id);
        Task<IEnumerable<AnnonceDTO>> GetAnnoncesFavoritesByCompteId(int compteId);
        Task<bool> EstMasquerAsync(int id);
        
        Task<AnnonceDetailDTO> GetAnnonceDetailById(int id);
    }

}
