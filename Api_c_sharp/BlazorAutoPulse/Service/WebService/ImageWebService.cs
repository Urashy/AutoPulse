using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service.WebService
{
    public class ImageWebService : BaseWebService<Image>
    {
        protected override string ApiEndpoint => "Image";
    }
}
