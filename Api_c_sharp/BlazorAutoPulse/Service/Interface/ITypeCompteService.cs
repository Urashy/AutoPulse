using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ITypeCompteService : IService<TypeCompte>
    {
        Task<IEnumerable<TypeCompte>> GetTypeComptesPourChercher();
    }
}
