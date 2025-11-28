using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class MarqueWebService : BaseWebService<Marque>
    {
        public MarqueWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Marque";
    }
}
