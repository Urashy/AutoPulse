using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class VoitureWebService: BaseWebService<Voiture>
{
    public VoitureWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Voiture";
}