using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.WebService
{
    public class PostImageWebService: BaseWebService<ImageUpload>, IPostImageService
    {
        public PostImageWebService(IHttpClientFactory factory) : base(factory)
        {
        }

        protected override string ApiEndpoint => "Image";

        public async Task<ImageDTO> CreateAsync(ImageUpload entity)
        {
            using var content = new MultipartFormDataContent();

            // ✅ CORRECTION: Utiliser ImageBytes au lieu de File.OpenReadStream()
            if (entity.ImageBytes != null && entity.ImageBytes.Length > 0)
            {
                // Créer le contenu depuis les bytes
                var fileContent = new ByteArrayContent(entity.ImageBytes);
                
                // Définir le content type
                var fileName = entity.File?.Name ?? "image.jpg";
                var contentType = entity.File.ContentType ?? GetContentType(fileName);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                
                content.Add(fileContent, "File", fileName);
            }
            else if (entity.File != null)
            {
                // ⚠️ Fallback: si ImageBytes n'est pas disponible, essayer avec File
                // (mais cela peut échouer si appelé hors du contexte du composant)
                Console.WriteLine("⚠️ Warning: Using File.OpenReadStream() as fallback. Consider preloading ImageBytes.");
                try
                {
                    var stream = entity.File.OpenReadStream(10 * 1024 * 1024); // 10MB max
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(entity.File.ContentType);
                    content.Add(streamContent, "File", entity.File.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error opening file stream: {ex.Message}");
                    throw new InvalidOperationException("Unable to read file. ImageBytes should be preloaded.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("Neither ImageBytes nor File is available for upload.");
            }

            content.Add(new StringContent(entity.IdImage.ToString()), "IdImage");
            content.Add(new StringContent(entity.IdVoiture?.ToString() ?? ""), "IdVoiture");
            content.Add(new StringContent(entity.IdCompte?.ToString() ?? ""), "IdCompte");
    
            var response = await _httpClient.PostAsync(BuildUrl("Post"), content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ImageDTO>();
        }
        
        public async Task UpdateAsync(int id, ImageUpload entity)
        {
            using var content = new MultipartFormDataContent();

            // ✅ CORRECTION: Utiliser ImageBytes au lieu de File.OpenReadStream()
            if (entity.ImageBytes != null && entity.ImageBytes.Length > 0)
            {
                // Créer le contenu depuis les bytes
                var fileContent = new ByteArrayContent(entity.ImageBytes);
                
                // Définir le content type
                var fileName = entity.File?.Name ?? "image.jpg";
                var contentType = entity.File.ContentType ?? GetContentType(fileName);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                
                content.Add(fileContent, "File", fileName);
            }
            else if (entity.File != null)
            {
                // ⚠️ Fallback: si ImageBytes n'est pas disponible
                Console.WriteLine("⚠️ Warning: Using File.OpenReadStream() as fallback for update.");
                try
                {
                    var stream = entity.File.OpenReadStream(10 * 1024 * 1024);
                    var streamContent = new StreamContent(stream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(entity.File.ContentType);
                    content.Add(streamContent, "File", entity.File.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error opening file stream: {ex.Message}");
                    throw new InvalidOperationException("Unable to read file. ImageBytes should be preloaded.", ex);
                }
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

        /// <summary>
        /// Détermine le Content-Type basé sur l'extension du fichier
        /// </summary>
        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                ".svg" => "image/svg+xml",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };
        }
    }
}