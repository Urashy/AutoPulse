using System.Net;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.Authentification;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public interface IServiceConnexion
{
    Task<HttpResponseMessage> LoginUser(LoginRequest compte);
    Task<HttpStatusCode> LogOutUser();
    Task<(HttpStatusCode StatusCode, HttpResponseMessage Response)> ValidateA2fLogin(TokenEmailVerifDTO dto);
    Task<GoogleLoginResponse> GoogleLogin();
}