using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface ICommandeService : IService<CommandeDTO>
    {
        Task<IEnumerable<CommandeDTO>> GetCommandeByCompte(int id);
        Task<CommandeDetailDTO> GetCommandeDetailById(int id);
        Task UpdateCommandeAsync(int id, CommandeUpdateDTO entity);
        Task<CommandeDTO> GetCommandeByIdConv(int id);
        Task<bool> TelechargerFacturePdf(int idCommande);
    }
}
