using System.Net.Http.Json;
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

        public async Task<Image> GetImageProfil(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{_httpClient.BaseAddress}{ApiEndpoint}/GetImageByCompte/{id}");
    
            var response = await SendWithCredentialsAsync(request);

            response.EnsureSuccessStatusCode();

            var image = await response.Content.ReadFromJsonAsync<Image>();

            return image!;
        }
    }
}
