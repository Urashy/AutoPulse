using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class MotriciteWebService: BaseWebService<Motricite>
{
    protected override string ApiEndpoint => "Motricite";
}