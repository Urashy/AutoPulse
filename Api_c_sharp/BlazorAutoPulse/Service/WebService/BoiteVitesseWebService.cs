using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class BoiteVitesseWebService: BaseWebService<BoiteDeVitesseDTO>
{
    public BoiteVitesseWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "BoiteDeVitesse";
}