using System.Net;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.Authentification;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public interface IServiceConnexion
{
    Task<HttpResponseMessage> LoginUser(LoginRequest compte, bool rememberMe = false);
    Task<HttpStatusCode> LogOutUser();
    Task<(HttpStatusCode StatusCode, HttpResponseMessage Response)> ValidateA2fLogin(TokenEmailVerifDTO dto, bool rememberMe = false);
    Task<GoogleLoginResponse> GoogleLogin();
}