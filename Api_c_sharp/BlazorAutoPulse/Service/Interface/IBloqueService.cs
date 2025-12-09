using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IBloqueService: IService<BloqueDTO>
{
    Task<bool> ABloque(int idBloquant, int idBloque, bool premierBloque);
    Task DeleteBloque(int idBloquant, int idBloque);
}