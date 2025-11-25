using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CarburantWebService : BaseWebService<Carburant>
    {
        public CarburantWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "carburant";
    }
}
