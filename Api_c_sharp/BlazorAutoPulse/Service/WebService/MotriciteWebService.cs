using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class MotriciteWebService: BaseWebService<MotriciteDTO>
{
    public MotriciteWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Motricite";
}