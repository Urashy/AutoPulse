using System.Net.Http.Json;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service;

public class ConnexionWebService: IServiceConnexion
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5086/api/Login/")
    };
    
    public async Task<Compte> LoginUser(LoginRequest compte)
    {
        var response = await _httpClient.PostAsJsonAsync<LoginRequest>("Login", compte);
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Console.WriteLine(loginResponse.UserDetails);

        return loginResponse?.UserDetails;
    }
}