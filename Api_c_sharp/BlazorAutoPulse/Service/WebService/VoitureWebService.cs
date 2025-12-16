using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class VoitureWebService: BaseWebService<VoitureDetailDTO>
{
    public VoitureWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Voiture";
}