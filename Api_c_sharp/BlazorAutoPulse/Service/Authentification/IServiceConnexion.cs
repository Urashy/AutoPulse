using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public interface IServiceConnexion
{
    Task<Compte> LoginUser(LoginRequest compte);
    Task<Compte> CreateUser(Compte compte);
}