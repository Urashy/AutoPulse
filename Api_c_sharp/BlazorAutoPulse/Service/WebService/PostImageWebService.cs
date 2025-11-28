using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http;
using System.Net.Http.Json;

namespace BlazorAutoPulse.Service.WebService
{
    public class PostImageWebService: BaseWebService<ImageUpload>, IPostImageService
    {
        public PostImageWebService(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override string ApiEndpoint => "Image";

        public async Task<Image> CreateAsync(ImageUpload entity)
        {
            using var content = new MultipartFormDataContent();

            if (entity.File != null)
            {
                content.Add(new StreamContent(entity.File.OpenReadStream()), "File", entity.File.Name);
            }

            content.Add(new StringContent(entity.IdImage.ToString()), "IdImage");
            content.Add(new StringContent(entity.IdVoiture?.ToString() ?? ""), "IdVoiture");
            content.Add(new StringContent(entity.IdCompte?.ToString() ?? ""), "IdCompte");

            var response = await _httpClient.PostAsync(BuildUrl("Post"), content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Image>();
        }

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
