using System.Net;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.Authentification;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class ConnexionViewModel
{
    private readonly IServiceConnexion _serviceConnexion;
    private Action? _stateHasChanged;
    private NavigationManager _nav;

    // Propriétés du formulaire
    public string emailUtilisateur { get; set; } = "";
    public string motDePasseUtilisateur { get; set; } = "";
    public bool rememberMe { get; set; } = false;
    public string messageErreur { get; set; } = "";
    public bool isLoading { get; set; } = false;

    // État A2F
    public bool requiresA2f { get; set; } = false;
    public bool mustReactivateA2f { get; set; } = false;
    public string codeA2f { get; set; } = "";
    private int userId;
    private string userEmail = "";

    public ConnexionViewModel(
        IServiceConnexion serviceConnexion)
    {
        _serviceConnexion = serviceConnexion;
    }

    public async Task InitializeAsync(Action stateHasChanged, NavigationManager navigation)
    {
        _stateHasChanged = stateHasChanged;
        _nav = navigation;
        await Task.CompletedTask;
    }

    public void Reset()
    {
        emailUtilisateur = "";
        motDePasseUtilisateur = "";
        rememberMe = false;
        messageErreur = "";
        isLoading = false;
        requiresA2f = false;
        mustReactivateA2f = false;
        codeA2f = "";
        userId = 0;
        userEmail = "";
    }
    
    public void SetNavigation(NavigationManager nav)
    {
        _nav = nav;
    }

    public async Task ConnexionUtilisateur()
    {
        if (string.IsNullOrWhiteSpace(emailUtilisateur) || string.IsNullOrWhiteSpace(motDePasseUtilisateur))
        {
            messageErreur = "Veuillez remplir tous les champs";
            _stateHasChanged?.Invoke();
            return;
        }

        isLoading = true;
        messageErreur = "";
        _stateHasChanged?.Invoke();

        try
        {
            var loginRequest = new LoginRequest
            {
                Email = emailUtilisateur,
                MotDePasse = motDePasseUtilisateur
            };

            var response = await _serviceConnexion.LoginUser(loginRequest, rememberMe);

            // ✅ CORRECTION: Vérifier d'abord le code 202 (Accepted) pour l'A2F
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                Console.WriteLine("✅ Code A2F requis");
                
                // Code A2F requis
                var content = await response.Content.ReadFromJsonAsync<A2fResponse>();
                
                if (content != null)
                {
                    requiresA2f = true;
                    mustReactivateA2f = content.MustReactivate;
                    userId = content.UserId;
                    userEmail = content.Email ?? emailUtilisateur;

                    messageErreur = "";
                    
                    Console.WriteLine($"A2F activé: requiresA2f={requiresA2f}, mustReactivate={mustReactivateA2f}, userId={userId}");
                }
                else
                {
                    messageErreur = "Erreur lors de la récupération des informations A2F";
                }
            }
            else if (response.IsSuccessStatusCode)
            {
                // Connexion réussie sans A2F (200 OK)
                Console.WriteLine("✅ Connexion réussie sans A2F");
                _nav?.NavigateTo("/", forceLoad: true);
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                messageErreur = "Email ou mot de passe incorrect";
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Erreur inattendue: {response.StatusCode} - {errorContent}");
                messageErreur = "Une erreur est survenue. Veuillez réessayer.";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Exception dans ConnexionUtilisateur: {ex.Message}");
            messageErreur = "Erreur de connexion. Vérifiez votre connexion internet.";
        }
        finally
        {
            isLoading = false;
            _stateHasChanged?.Invoke();
        }
    }
    
    public async Task DeconnexionUtilisateur()
    {
        try
        {
            await _serviceConnexion.LogOutUser();
            await Task.Delay(100);
            _nav.NavigateTo("/connexion", forceLoad: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur DeconnexionUtilisateur: {ex.Message}");
            _nav.NavigateTo("/connexion", forceLoad: true);
        }
    }

    public async Task ValiderCodeA2f()
    {
        if (string.IsNullOrWhiteSpace(codeA2f) || codeA2f.Length != 7)
        {
            messageErreur = "Veuillez saisir un code à 7 chiffres";
            _stateHasChanged?.Invoke();
            return;
        }

        isLoading = true;
        messageErreur = "";
        _stateHasChanged?.Invoke();

        try
        {
            var dto = new TokenEmailVerifDTO
            {
                Email = userEmail,
                Code = codeA2f,
                TypeToken = mustReactivateA2f ? "A2F_ACTIVATION" : "A2F_CONNEXION"
            };

            // ✅ Passer rememberMe au service
            var (statusCode, response) = await _serviceConnexion.ValidateA2fLogin(dto, rememberMe);

            if (statusCode == HttpStatusCode.OK)
            {
                _nav?.NavigateTo("/", forceLoad: true);
            }
            else
            {
                messageErreur = "Code invalide ou expiré";
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de la validation. Veuillez réessayer.";
        }
        finally
        {
            isLoading = false;
            _stateHasChanged?.Invoke();
        }
    }

    public async Task RenvoyerCodeA2f()
    {
        isLoading = true;
        messageErreur = "";
        _stateHasChanged?.Invoke();

        try
        {
            // Appeler l'endpoint pour renvoyer le code
            var loginRequest = new LoginRequest
            {
                Email = userEmail,
                MotDePasse = motDePasseUtilisateur
            };

            var response = await _serviceConnexion.LoginUser(loginRequest, rememberMe);

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                messageErreur = ""; // Pas d'erreur, mais on peut afficher un message de succès
                codeA2f = ""; // Réinitialiser le champ
            }
            else
            {
                messageErreur = "Impossible de renvoyer le code";
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de l'envoi du code";
        }
        finally
        {
            isLoading = false;
            _stateHasChanged?.Invoke();
        }
    }

    public async Task ConnecterAvecGoogle()
    {
        try
        {
            var response = await _serviceConnexion.GoogleLogin();

            if (!string.IsNullOrEmpty(response?.Url))
            {
                _nav?.NavigateTo(response.Url, forceLoad: true);
            }
            else
            {
                messageErreur = "Impossible de se connecter avec Google";
                _stateHasChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Impossible de se connecter avec Google";
            _stateHasChanged?.Invoke();
        }
    }
}