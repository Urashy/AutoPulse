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
    /// Récupère tous les signalements avec leurs détails
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
    /// Met à jour l'état d'un signalement (1=En attente, 2=Traité, 3=Rejeté)
    /// </summary>
    public async Task<bool> UpdateEtatAsync(int idSignalement, int nouvelEtat)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Put,
                BuildUrl($"UpdateEtat/{idSignalement}/{nouvelEtat}")
            );

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Erreur UpdateEtat : {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception UpdateEtatAsync : {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Crée un signalement (annonce ou compte)
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
    /// Récupère les signalements filtrés par état, type (dans le chemin de l'URL) et recherche textuelle (en query string).
    /// </summary>
    public async Task<IEnumerable<SignalementDTO>> GetFiltered(int idetat, int idtype, string recherche)
    {
        try
        {
            var rechercheEncoded = Uri.EscapeDataString(recherche ?? string.Empty);

            // L'URL est de la forme: Signalement/{idetat}/{idtype}?recherche=...
            var url = $"{idetat}/{idtype}";
            if (!string.IsNullOrEmpty(rechercheEncoded))
            {
                url += $"?recherche={rechercheEncoded}";
            }

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                // Utilise BuildUrl pour ajouter le préfixe de l'API (Signalement)
                BuildUrl(url)
            );

            var response = await SendWithCredentialsAsync(request);

            // Lance une exception pour les codes d'état HTTP non réussis (4xx ou 5xx)
            response.EnsureSuccessStatusCode();

            // Désérialiser la réponse en IEnumerable<SignalementDTO>
            return await response.Content.ReadFromJsonAsync<IEnumerable<SignalementDTO>>()
                   ?? Enumerable.Empty<SignalementDTO>();
        }
        catch (Exception ex)
        {
            // Gérer les erreurs de désérialisation, de connexion, ou de code d'état HTTP non réussi
            Console.WriteLine($"Exception GetFiltered : {ex.Message}");
            return Enumerable.Empty<SignalementDTO>();
        }
    }
}