using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class ConversationWebService: BaseWebService<ConversationListDTO>, IConversationService
{
    public ConversationWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "Conversation";
    public async Task<IEnumerable<ConversationListDTO>> GetConversationsByCompteID(int compteId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetConversationsByCompteID/{compteId}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ConversationListDTO>>();
    }

    public async Task<ConversationListDTO> PostComplet(ConversationCreateDTO conversation, int idCompteEnvoie, int idCompteRecoi)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"Post/{idCompteEnvoie}/{idCompteRecoi}"))
        {
            Content = JsonContent.Create(conversation)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<ConversationListDTO>();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return await response.Content.ReadFromJsonAsync<ConversationListDTO>();
        }
    }
}