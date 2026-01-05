using System.Net.Http.Json;
using System.Text.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.Service;

public class CompteWebService : BaseWebService<CompteDetailDTO>, ICompteService
{
    public CompteWebService(HttpClient httpClient) : base(httpClient)
    {
    }

    protected override string ApiEndpoint => "Compte";
    public async Task<IEnumerable<CompteGetDTO>> GetAllAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetAll"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<CompteGetDTO>>();
    }

    public async Task<ServiceResult<CompteCreateDTO>> PostWithErrorHandlingAsync(CompteCreateDTO compte)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("Post"))
            {
                Content = JsonContent.Create(compte)
            };

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<CompteCreateDTO>();
                return ServiceResult<CompteCreateDTO>.SuccessResult(result);
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

                    if (validationError?.Errors != null && validationError.Errors.Any())
                    {
                        var errorMessages = validationError.Errors
                            .SelectMany(e => e.Value)
                            .ToList();
                        
                        return ServiceResult<CompteCreateDTO>.ErrorResult(
                            string.Join("\n", errorMessages),
                            validationError.Errors
                        );
                    }
                }
                catch (JsonException)
                {
                    return ServiceResult<CompteCreateDTO>.ErrorResult("Erreur de validation : " + errorContent);
                }
            }

            return ServiceResult<CompteCreateDTO>.ErrorResult($"Erreur {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception PostWithErrorHandlingAsync : {ex.Message}");
            return ServiceResult<CompteCreateDTO>.ErrorResult("Une erreur s'est produite");
        }
    }

    public async Task<CompteDetailDTO> GetByNameAsync(string name)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByString/{name}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompteDetailDTO>();
    }

    public async Task<CompteDetailDTO> GetMe()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl("GetMe"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompteDetailDTO>();
    }

    public async Task<IEnumerable<CompteGetDTO>> GetByTypeCompteAsync(int idTypeCompte)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetByTypeCompte/{idTypeCompte}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IEnumerable<CompteGetDTO>>();
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
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"PutAnonymise/{idCompte}"));
        
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

    public async Task<bool> PutTypeCompte(int idCompte, CompteModifTypeCompteDTO compte)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"PutTypeCompte/{idCompte}"))
        {
            Content = JsonContent.Create(compte)
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
    
    public async Task<CompteProfilPublicDTO> GetComptePublicById(int id)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, BuildUrl($"GetProfilPublic/{id}"));
        var response = await SendWithCredentialsAsync(request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CompteProfilPublicDTO>();
    }

    public async Task<bool> ToggleSuspention(int idCompte,bool e)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, BuildUrl($"ToggleEtatCompte/{idCompte}/{e}"));

            var response = await SendWithCredentialsAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Erreur ToggleSuspention : {error}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception ToggleSuspention : {ex.Message}");
            return false;
        }
    }

}