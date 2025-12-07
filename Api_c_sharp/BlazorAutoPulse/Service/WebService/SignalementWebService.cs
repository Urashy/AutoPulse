using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class SignalementWebService : BaseWebService<SignalementAnnonceCreateDTO>, ISignalementService
{
    public SignalementWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Signalement";

    public async Task<IEnumerable<SignalementDTO>> GetAllAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<SignalementDTO>>()
               ?? Enumerable.Empty<SignalementDTO>();
    }

    public async Task<bool> PostCompte(SignalementCreateDTO signalement)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"Post"))
        {
            Content = JsonContent.Create(signalement)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return false;
        }
    }
    public async Task<bool> UpdateEtatAsync(int idSignalement, int nouvelEtat)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                BuildUrl($"UpdateEtat/{idSignalement}/{nouvelEtat}")
            );

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur UpdateEtat : {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception UpdateEtatAsync : {ex.Message}");
            return false;
        }
    }
}