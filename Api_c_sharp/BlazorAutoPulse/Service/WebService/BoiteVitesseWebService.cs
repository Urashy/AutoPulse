using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class BoiteVitesseWebService: BaseWebService<BoiteDeVitesseDTO>
{
    public BoiteVitesseWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "BoiteDeVitesse";
}