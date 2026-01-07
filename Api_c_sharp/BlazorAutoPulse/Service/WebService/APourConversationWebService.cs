using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class APourConversationWebService: BaseWebService<APourConversationDTO>, IAPourConversationService
{
    public APourConversationWebService(IHttpClientFactory factory) : base(factory)
    {
    }
    
    protected override string ApiEndpoint => "APourConversation";

    public async Task<bool> ConvExist(int idCompteUn, int idCompteDeux, int idAnnonce)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"Exists/{idCompteUn}/{idCompteDeux}/{idAnnonce}"));
            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur APourConversationExists : {error}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception APourConversationExists : {ex.Message}");
            return false;
        }
    }

}