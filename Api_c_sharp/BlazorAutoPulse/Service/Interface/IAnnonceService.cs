using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAnnonceService : IService<Annonce>
    {
        Task<IEnumerable<Annonce>> GetByIdMiseEnAvant(int id);
        Task<IEnumerable<Annonce>> GetFilteredAnnoncesAsync(ParametreRecherche searchParams);
    }

    public interface IAnnonceDetailService : IService<AnnonceDetailDTO>
    {
    }

}
