using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    public interface IPlainteService
    {
        Task<PlainteDTO> CreatePlainteAsync(PlainteCreateDTO entity);

    }
}