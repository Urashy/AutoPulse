using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class AdresseWebService: BaseWebService<Adresse>
{
    protected override string ApiEndpoint => "Adresse";
}