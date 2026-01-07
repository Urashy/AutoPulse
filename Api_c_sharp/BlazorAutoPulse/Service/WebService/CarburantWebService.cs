using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class CarburantWebService : BaseWebService<CarburantDTO>
    {
        public CarburantWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Carburant";
    }
}
