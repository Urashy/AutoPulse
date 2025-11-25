using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class BoiteVitesseWebService: BaseWebService<BoiteDeVitesse>
{
    public BoiteVitesseWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "BoiteDeVitesse";
}