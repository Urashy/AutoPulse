using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface ITokenEmailService
{
    Task<bool> EnvoyerToken(TokenEmailCreateDTO dto);
    Task<bool> VerifierCode(TokenEmailVerifDTO dto);
    Task<bool> MarquerUtilise(int idToken);
}