using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class MarqueWebService : BaseWebService<Marque>
    {
        public MarqueWebService(HttpClient httpClient) : base(httpClient)
        {
            Console.WriteLine(httpClient.BaseAddress);
        }

        protected override string ApiEndpoint => "Marque";
    }
}
