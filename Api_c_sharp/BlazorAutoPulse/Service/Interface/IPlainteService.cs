using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface
{
    // Assumons que ServiceResult est utilisé pour gérer la réponse
    public interface IPlainteService
    {
        Task<ServiceResult> EnvoyerPlainteAsync(PlainteCreateDTO plainteDTO);
    }
}