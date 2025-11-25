using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class MotriciteWebService: BaseWebService<Motricite>
{
    public MotriciteWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Motricite";
}