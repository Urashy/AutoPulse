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
        
        public async Task UpdateAsync(int id, ImageUpload entity)
        {
            using var content = new MultipartFormDataContent();

            // Ajouter le fichier si présent
            if (entity.File != null)
            {
                content.Add(new StreamContent(entity.File.OpenReadStream()), "File", entity.File.Name);
            }

            // Ajouter les champs simples
            content.Add(new StringContent(entity.IdImage.ToString()), "IdImage");
            content.Add(new StringContent(entity.IdVoiture?.ToString() ?? ""), "IdVoiture");
            content.Add(new StringContent(entity.IdCompte?.ToString() ?? ""), "IdCompte");

            // Appel PUT vers Put/{id}
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            
            return;
        }
    }
}
