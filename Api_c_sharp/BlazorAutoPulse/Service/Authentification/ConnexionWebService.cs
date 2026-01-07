using System.Net;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.Authentification;
using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace BlazorAutoPulse.Service.Authentification;

public class ConnexionWebService : IServiceConnexion
{
    private readonly HttpClient _httpClient;

    public ConnexionWebService(IHttpClientFactory factory)
    {
        _httpClient = factory.CreateClient("ApiClient");
    }

    public async Task<HttpResponseMessage> LoginUser(LoginRequest compte, bool rememberMe = false)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"Compte/Login?rememberMe={rememberMe}")
        {
            Content = JsonContent.Create(compte)
        };

        // CRITIQUE : Toujours inclure les cookies
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var response = await _httpClient.SendAsync(request);

        return response;
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
    
    public async Task<(HttpStatusCode StatusCode, HttpResponseMessage Response)> ValidateA2fLogin(
        TokenEmailVerifDTO dto, bool rememberMe = false)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Post, 
                $"Compte/ValidateA2fLogin?rememberMe={rememberMe}")
            {
                Content = JsonContent.Create(dto)
            };
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await _httpClient.SendAsync(request);

            return (response.StatusCode, response);
        }
        catch (Exception ex)
        {
            return (HttpStatusCode.InternalServerError, null);
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