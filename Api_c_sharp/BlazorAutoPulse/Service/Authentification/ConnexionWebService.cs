using System.Net;
using System.Net.Http.Json;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.Authentification;

public class ConnexionWebService : IServiceConnexion
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://localhost:7295/api/Compte/")
    };

    public async Task<HttpStatusCode> LoginUser(LoginRequest compte)
    {
        var response = await _httpClient.PostAsJsonAsync("Login", compte);
        response.EnsureSuccessStatusCode();
        return response.StatusCode;
    }
}