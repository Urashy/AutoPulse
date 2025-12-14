using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService
{
    public class VueWebService : BaseWebService<VueDTO>
    {
        public VueWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Vue";
    }
}
