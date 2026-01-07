using BlazorAutoPulse.Model;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class APourCouleurWebService: BaseWebService<APourCouleurDTO>
{
    public APourCouleurWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "APourCouleur";
}