using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Interface;

public interface IModeleService : IService<ModeleDTO>
{
    Task<IEnumerable<ModeleDTO>> FiltreModeleParMarque(int idMarque);
}