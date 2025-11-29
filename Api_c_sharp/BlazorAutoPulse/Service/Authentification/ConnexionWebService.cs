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
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "Compte/Login")
            {
                Content = JsonContent.Create(compte)
            };
            
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            
            // Ne pas lancer d'exception si 401 (credentials invalides)
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return HttpStatusCode.Unauthorized;
            }
            
            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return HttpStatusCode.BadRequest;
            }
            
            response.EnsureSuccessStatusCode();
            return response.StatusCode;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erreur LoginUser: {ex.Message}");
            return HttpStatusCode.InternalServerError;
        }
    }

    public async Task<HttpStatusCode> LogOutUser()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "Compte/Logout");
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            var response = await _httpClient.SendAsync(request);
            
            // Même si le logout échoue, on considère que c'est OK côté client
            return response.IsSuccessStatusCode ? response.StatusCode : HttpStatusCode.OK;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Erreur LogOutUser: {ex.Message}");
            // On retourne OK même en cas d'erreur car le cookie sera supprimé côté client
            return HttpStatusCode.OK;
        }
    }
    
    public async Task<GoogleLoginResponse> GoogleLogin()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "Compte/GoogleLogin");
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
    }
}