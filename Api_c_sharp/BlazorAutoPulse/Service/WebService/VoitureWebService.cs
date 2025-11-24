using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class VoitureWebService: BaseWebService<Voiture>
{
    protected override string ApiEndpoint => "Voiture";
}