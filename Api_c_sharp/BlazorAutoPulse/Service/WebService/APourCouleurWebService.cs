using BlazorAutoPulse.Model;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class APourCouleurWebService: BaseWebService<APourCouleurDTO>
{
    public APourCouleurWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "APourCouleur";
}