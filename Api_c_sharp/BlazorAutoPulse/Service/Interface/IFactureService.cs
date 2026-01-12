using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFactureService : IService<CommandeDTO>
    {

        Task<bool> TelechargerFacturePdf(int commandeId); // Celle que vous aviez
        Task<Stream?> GetFactureStreamAsync(int commandeId);
    }
}
