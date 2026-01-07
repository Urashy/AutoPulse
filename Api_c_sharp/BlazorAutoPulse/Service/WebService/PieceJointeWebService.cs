using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAutoPulse.Service.WebService;

public class PieceJointeWebService : BaseWebService<PieceJointeDTO>, IPieceJointeService
{
    private const long MaxFileSize = 10 * 1024 * 1024;

    public PieceJointeWebService(IHttpClientFactory factory) : base(factory)
    {
    }

    protected override string ApiEndpoint => "PieceJointe";

    /// <summary>
    /// Upload des fichiers en les encodant en Base64
    /// </summary>
    public async Task<List<PieceJointeDTO>> UploadFilesAsync(int idMessage, IReadOnlyList<IBrowserFile> files)
    {
        var uploadDtos = new List<PieceJointeUploadDTO>();

        foreach (var file in files)
        {
            try
            {
                // Lire le fichier et le convertir en Base64
                using var stream = file.OpenReadStream(MaxFileSize);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var fileBytes = memoryStream.ToArray();
                var base64String = Convert.ToBase64String(fileBytes);

                var uploadDto = new PieceJointeUploadDTO
                {
                    IdMessage = idMessage,
                    NomFichier = file.Name,
                    TypeMime = file.ContentType,
                    Extension = Path.GetExtension(file.Name).ToLowerInvariant(),
                    TailleFichier = file.Size,
                    ContenuBase64 = base64String
                };

                uploadDtos.Add(uploadDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lecture fichier {file.Name}: {ex.Message}");
            }
        }

        if (!uploadDtos.Any())
            return new List<PieceJointeDTO>();

        var response = await _httpClient.PostAsJsonAsync(BuildUrl("Upload"), uploadDtos);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<PieceJointeDTO>>() ?? new List<PieceJointeDTO>();
    }

    /// <summary>
    /// Récupérer une pièce jointe avec son contenu Base64
    /// </summary>
    public async Task<PieceJointeDTO?> GetByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync(BuildUrl($"GetById/{id}"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PieceJointeDTO>();
    }

    /// <summary>
    /// Récupérer les pièces jointes d'un message avec leur contenu Base64
    /// </summary>
    public async Task<IEnumerable<PieceJointeDTO>> GetByMessageAsync(int idMessage)
    {
        var response = await _httpClient.GetAsync(BuildUrl($"GetByMessage/{idMessage}"));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<PieceJointeDTO>>() ?? Array.Empty<PieceJointeDTO>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"api/PieceJointe/Delete/{id}");
        return response.IsSuccessStatusCode;
    }
} 