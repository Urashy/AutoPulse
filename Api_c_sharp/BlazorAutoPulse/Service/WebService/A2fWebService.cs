using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class A2fWebService : BaseWebService<A2fStatutDTO>, IA2fService
{
    public A2fWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Compte";

    public async Task<A2fStatutDTO> GetStatutA2f(int idCompte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetStatutA2f/{idCompte}"));

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<A2fStatutDTO>();
            }

            return new A2fStatutDTO { A2fActif = false, DoitReactiver = false };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur GetStatutA2f : {ex.Message}");
            return new A2fStatutDTO { A2fActif = false, DoitReactiver = false };
        }
    }

    public async Task<bool> VerifActivA2f(int idCompte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"VerifActivA2f/{idCompte}"));

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur VerifActivA2f : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DemanderActivationA2f(int idCompte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"DemanderActivationA2f/{idCompte}"));

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur DemanderActivationA2f : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ActiverA2f(A2fActivationDTO dto)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("ActiverA2f"))
            {
                Content = JsonContent.Create(dto)
            };

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur ActiverA2f : {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DesactiverA2f(int idCompte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"DesactiverA2f/{idCompte}"));

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur DesactiverA2f : {ex.Message}");
            return false;
        }
    }
}