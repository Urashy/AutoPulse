using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class MotriciteWebService: BaseWebService<MotriciteDTO>
{
    public MotriciteWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "Motricite";
}