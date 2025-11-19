using System.Net.Http.Json;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public class ConnexionWebService : IServiceConnexion
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5086/api/Login/")
    };

    public async Task<Compte> LoginUser(LoginRequest compte)
    {
        var response = await _httpClient.PostAsJsonAsync("Login", compte);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        return loginResponse?.UserDetails;
    }

    public async Task<Compte> CreateUser(Compte compte)
    {
        var response = await _httpClient.PostAsJsonAsync("CreateUser", compte);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Compte>();
    }
}