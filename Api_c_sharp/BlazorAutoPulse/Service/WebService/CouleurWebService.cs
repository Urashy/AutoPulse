using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService;

public class CouleurWebService: BaseWebService<Couleur>
{
    public CouleurWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Couleur";
}