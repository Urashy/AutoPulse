using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService
{
    public class ImageWebService : BaseWebService<ImageDTO>, IImageService
    {
        public ImageWebService(IHttpClientFactory factory) : base(factory)
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

        public async Task<List<int>> GetAllImageIdsByVoitureId(int voitureId)
        {
            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Get,
                    BuildUrl($"GetAllImagesByVoitureId/{voitureId}")
                );

                var response = await SendWithCredentialsAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var imageIds = await response.Content.ReadFromJsonAsync<List<int>>();
                    return imageIds ?? new List<int>();
                }

                return new List<int>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur GetAllImageIdsByVoitureId: {ex.Message}");
                return new List<int>();
            }
        }

        public async Task<ImageDTO?> GetImageProfil(int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_httpClient.BaseAddress}{ApiEndpoint}/GetImageByCompte/{id}");

                var response = await SendWithCredentialsAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var image = await response.Content.ReadFromJsonAsync<ImageDTO>();
                    return image;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    Console.WriteLine($"Image pour le compte {id} non trouvée.");
                    return null;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"Utilisateur non autorisé pour récupérer l'image du compte {id}.");
                    return null;
                }
                Console.WriteLine($"Erreur HTTP {response.StatusCode} lors de la récupération de l'image du compte {id}.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception lors de la récupération de l'image du compte {id} : {ex.Message}");
                return null;
            }
        }
    }
}
