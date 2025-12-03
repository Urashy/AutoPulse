using System.Net.Http.Json;
using System.Text.Json;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class CompteWebService : BaseWebService<Compte>, ICompteService
{
    public CompteWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Compte";

    public async Task<Compte> GetByNameAsync(string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByString/{name}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Compte>();
    }

    public async Task<Compte> GetMe()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetMe"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Compte>();
    }

    public async Task<int?> GetTypeCompteByCompteId(int idCompte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetTypeCompteByCompteId/{idCompte}"));
            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Compte avec l'ID {idCompte} introuvable");
                return null;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur GetTypeCompteByCompteId : {error}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception GetTypeCompteByCompteId : {ex.Message}");
            return null;
        }
    }

    public async Task<bool> VerifUser(ChangementMdp changementMdp)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("VerifUser"))
        {
            Content = JsonContent.Create(changementMdp)
        };
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return false;
        }
    }

    // ✅ NOUVELLE MÉTHODE avec gestion d'erreur améliorée
    public async Task<ServiceResult<bool>> ChangementMdp(ChangementMdp changementMdp)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("ModifMdp"))
            {
                Content = JsonContent.Create(changementMdp)
            };
            
            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.SuccessResult(true);
            }
            
            // Gestion des erreurs de validation (400)
            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var validationError = JsonSerializer.Deserialize<ValidationErrorResponse>(
                        errorContent, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    // Construire un message d'erreur lisible
                    if (validationError?.Errors != null && validationError.Errors.Any())
                    {
                        var errorMessages = validationError.Errors
                            .SelectMany(e => e.Value)
                            .ToList();
                        
                        return ServiceResult<bool>.ErrorResult(
                            string.Join("\n", errorMessages),
                            validationError.Errors
                        );
                    }
                }
                catch (JsonException)
                {
                    // Si le JSON n'est pas au format attendu
                    return ServiceResult<bool>.ErrorResult("Erreur de validation : " + errorContent);
                }
            }

            // Autres erreurs HTTP
            return ServiceResult<bool>.ErrorResult($"Erreur {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception ChangementMdp : {ex.Message}");
            return ServiceResult<bool>.ErrorResult("Une erreur s'est produite lors du changement de mot de passe");
        }
    }

    public async Task<bool> Anonymisation(int idCompte)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl($"Anonymisation/{idCompte}"));
        
        var response = await SendWithCredentialsAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return true;
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur Post : {error}");
            return false;
        }
    }
}