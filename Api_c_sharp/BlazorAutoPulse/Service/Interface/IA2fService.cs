using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface IA2fService
{
    Task<A2fStatutDTO> GetStatutA2f(int idCompte);
    Task<bool> VerifActivA2f(int idCompte);
    Task<bool> DemanderActivationA2f(int idCompte);
    Task<bool> ActiverA2f(A2fActivationDTO dto);
    Task<bool> DesactiverA2f(int idCompte);
}