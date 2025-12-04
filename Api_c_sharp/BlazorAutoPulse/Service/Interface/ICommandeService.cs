using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ICommandeService : IService<CommandeDTO>
    {
        Task<IEnumerable<CommandeDTO>> GetCommandeByCompte(int id);
    }
}
