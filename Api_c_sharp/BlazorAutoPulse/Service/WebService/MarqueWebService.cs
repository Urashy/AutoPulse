using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class MarqueWebService : BaseWebService<Marque>
    {
        protected override string ApiEndpoint => "Marque";
    }
}
