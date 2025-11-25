using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class ImageWebService : BaseWebService<Image>
    {
        public ImageWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Image";
    }
}
