using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class MarqueWebService : BaseWebService<MarqueDTO>
    {
        public MarqueWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Marque";
    }
}
