using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IFactureService : IService<CommandeDTO>
    {

        Task<Stream?> GetFactureStreamAsync(int commandeId);
    }
}
