using System.Net;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public interface IServiceConnexion
{
    Task<HttpStatusCode> LoginUser(LoginRequest compte);
    Task<HttpStatusCode> LogOutUser();
}