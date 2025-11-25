using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class APourCouleurWebService: BaseWebService<APourCouleur>
{
    protected override string ApiEndpoint => "APourCouleur";
}