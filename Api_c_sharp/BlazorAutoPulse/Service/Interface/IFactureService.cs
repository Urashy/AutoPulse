using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFactureService : IService<CommandeDTO>
    {
        Task<bool> TelechargerFacturePdf(int idCommande,bool download = true);
    }
}
