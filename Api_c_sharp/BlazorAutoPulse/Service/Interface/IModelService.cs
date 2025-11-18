using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface IModeleService : IService<Modele>
{
    Task<IEnumerable<Modele>> FiltreModeleParMarque(int idMarque);
}