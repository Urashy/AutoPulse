using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class ConversationWebService: BaseWebService<ConversationDetailDTO>
{
    public ConversationWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Conversation";
}