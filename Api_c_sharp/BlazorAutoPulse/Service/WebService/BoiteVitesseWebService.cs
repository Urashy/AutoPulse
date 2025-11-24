using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class BoiteVitesseWebService: BaseWebService<BoiteDeVitesse>
{
    protected override string ApiEndpoint => "BoiteDeVitesse";
}