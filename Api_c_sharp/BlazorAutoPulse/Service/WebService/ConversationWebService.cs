using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class ConversationWebService: BaseWebService<ConversationDetailDTO>, IConversationService
{
    public ConversationWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Conversation";
    public async Task<IEnumerable<ConversationDetailDTO>> GetConversationsByCompteID(int compteId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetConversationsByCompteID/{compteId}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<ConversationDetailDTO>>();
    }
}