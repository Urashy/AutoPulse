using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CarburantWebService : BaseWebService<Carburant>
    {
        protected override string ApiEndpoint => "carburant";
    }
}
