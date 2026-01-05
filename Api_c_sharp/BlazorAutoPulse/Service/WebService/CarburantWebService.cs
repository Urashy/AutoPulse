using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CarburantWebService : BaseWebService<CarburantDTO>
    {
        public CarburantWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Carburant";
    }
}
