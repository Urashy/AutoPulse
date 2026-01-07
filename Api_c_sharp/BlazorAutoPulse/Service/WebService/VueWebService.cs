using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService
{
    public class VueWebService : BaseWebService<VueDTO>
    {
        public VueWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Vue";
    }
}
