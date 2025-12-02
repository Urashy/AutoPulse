using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class MessageWebService: BaseWebService<MessageDTO>
{
    public MessageWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Message";
}