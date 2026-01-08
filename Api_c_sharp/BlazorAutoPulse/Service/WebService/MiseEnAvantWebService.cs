using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService;

public class MiseEnAvantWebService: BaseWebService<MiseEnAvantDTO>
{
    public MiseEnAvantWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "MiseEnAvant";
}