using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service;

public interface IServiceConnexion
{
    Task<Compte> LoginUser(LoginRequest compte);
}