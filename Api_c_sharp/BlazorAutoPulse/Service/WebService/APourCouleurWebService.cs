using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class APourCouleurWebService: BaseWebService<APourCouleur>
{
    public APourCouleurWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "APourCouleur";
}