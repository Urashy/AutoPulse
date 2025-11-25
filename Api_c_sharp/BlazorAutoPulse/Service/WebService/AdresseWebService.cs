using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class AdresseWebService: BaseWebService<Adresse>
{
    public AdresseWebService(HttpClient httpClient) : base(httpClient)
    {
    }
    protected override string ApiEndpoint => "Adresse";
}