using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service.WebService;

public class SignalementWebService : BaseWebService<SignalementCreateDTO>, ISignalementService
{
    public SignalementWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Signalement";

    /// <summary>
    /// Récupère tous les signalements avec leurs détails.
    /// URL générée : api/Signalement/GetAll
    /// </summary>
    public async Task<IEnumerable<SignalementDTO>> GetAllSignalementsAsync()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));
            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<SignalementDTO>>()
                   ?? Enumerable.Empty<SignalementDTO>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur GetAllSignalementsAsync : {ex.Message}");
            return Enumerable.Empty<SignalementDTO>();
        }
    }

    /// <summary>
    /// Met à jour l'état d'un signalement.
    /// URL générée : api/Signalement/Put/{id}
    /// </summary>
    public async Task UpdateSignalementAsync(int id, SignalementUpdateDTO entity)
    {
        try
        {
            // Attention : Assure-toi que ton contrôleur a bien une action [ActionName("Put")]
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"Put/{id}"))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur UpdateSignalementAsync : {ex.Message}");
        }
    }

    /// <summary>
    /// Crée un signalement.
    /// URL générée : api/Signalement/Post
    /// </summary>
    public override async Task<SignalementCreateDTO> CreateAsync(SignalementCreateDTO entity)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
            {
                Content = JsonContent.Create(entity)
            };

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SignalementCreateDTO>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur CreateAsync : {error}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception CreateAsync : {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// CORRECTION ICI : Ajout du nom de l'action "GetFilteredSignalement" dans l'URL.
    /// URL générée : api/Signalement/GetFilteredSignalement/{idetat}/{idtype}?recherche=...
    /// </summary>
    public async Task<IEnumerable<SignalementDTO>> GetFiltered(int idetat, int idtype, string recherche)
    {
        try
        {
            var rechercheEncoded = Uri.EscapeDataString(recherche ?? string.Empty);

            // CORRECTION : On ajoute "GetFilteredSignalement/" avant les paramètres
            var url = $"GetFilteredSignalement/{idetat}/{idtype}";

            if (!string.IsNullOrEmpty(rechercheEncoded))
            {
                url += $"?recherche={rechercheEncoded}";
            }

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUrl(url)
            );

            var response = await SendWithCredentialsAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<SignalementDTO>>()
                   ?? Enumerable.Empty<SignalementDTO>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception GetFiltered : {ex.Message}");
            return Enumerable.Empty<SignalementDTO>();
        }
    }
}