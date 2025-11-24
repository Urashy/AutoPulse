using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class CouleurWebService: BaseWebService<Couleur>
{
    protected override string ApiEndpoint => "Couleur";
}