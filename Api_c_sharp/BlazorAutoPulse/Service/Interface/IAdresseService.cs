using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IAdresseService : IService<Adresse>
    {
        Task<IEnumerable<Adresse>> GetAdresseByCompte();
    }
}
