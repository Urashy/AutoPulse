using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class TokenEmailWebService : BaseWebService<TokenEmailDTO>, ITokenEmailService
{
    public TokenEmailWebService(HttpClient httpClient) : base(httpClient)
    {
    }
    protected override string ApiEndpoint => "TokenEmail";

    public async Task<bool> EnvoyerToken(TokenEmailCreateDTO dto)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
            {
                Content = JsonContent.Create(dto)
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur EnvoyerToken : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> VerifierCode(TokenEmailVerifDTO dto)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("VerifCode"))
            {
                Content = JsonContent.Create(dto)
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur VerifierCode : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MarquerUtilise(int idToken)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"MarquerUtilise/{idToken}"));

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur MarquerUtilise : {ex.Message}");
            return false;
        }
    }
}