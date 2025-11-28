using System.Net;
using System.Net.Http.Json;
using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAutoPulse.Service.Authentification;

public class ConnexionWebService : IServiceConnexion
{
    private readonly HttpClient _httpClient;

    public ConnexionWebService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpStatusCode> LoginUser(LoginRequest compte)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "Compte/Login")
        {
            Content = JsonContent.Create(compte)
        };
        
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _httpClient.SendAsync(request);
        
        response.EnsureSuccessStatusCode();
        return response.StatusCode;
    }

    public async Task<HttpStatusCode> LogOutUser()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "Compte/Logout");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        return response.StatusCode;
    }
}