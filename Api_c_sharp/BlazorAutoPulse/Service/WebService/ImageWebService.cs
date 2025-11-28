using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class ImageWebService : BaseWebService<Image>, IImageService
    {
        public ImageWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Image";
        
        public string GetImage(int id)
        {
            return $"{_httpClient.BaseAddress}{ApiEndpoint}/GetById/{id}";
        }

        public string GetFirstImage(int id)
        {
            return $"{_httpClient.BaseAddress}{ApiEndpoint}/GetFirstImage/{id}";
        }

        public string GetAllIdImage(int id)
        {
            throw new NotImplementedException();
        }

        public string GetImageProfil(int id)
        {
            return $"{_httpClient.BaseAddress}{ApiEndpoint}/GetImageByCompte/{id}";
        }
    }
}
