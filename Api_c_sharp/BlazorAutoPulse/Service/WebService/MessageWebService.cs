using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class MessageWebService: BaseWebService<MessageDTO>, IMessageService
{
    public MessageWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Message";

    public async Task<MessageCreateDTO> CreateMessageAsync(MessageCreateDTO messageCreateDTO)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
        {
            Content = JsonContent.Create(messageCreateDTO)
        };

        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<MessageCreateDTO>();
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return await response.Content.ReadFromJsonAsync<MessageCreateDTO>();
        }
    }

    public async Task<IEnumerable<MessageDTO>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetAllByConversationAndMarkAsRead/{conversationId}/{userId}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<MessageDTO>>();
    }
}