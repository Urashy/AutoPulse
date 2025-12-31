using System.Net;
using System.Net.Http.Json;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class ConnexionViewModel
{
    private readonly IServiceConnexion _connexionService;
    private readonly ICompteService _compteService;
    private readonly ITokenEmailService _tokenEmailService;

    public string emailUtilisateur { get; set; }
    public string motDePasseUtilisateur { get; set; }
    public string messageErreur { get; set; }
    public bool isLoading { get; set; }
    
    // Nouveaux champs pour A2F
    public bool requiresA2f { get; set; }
    public bool mustReactivateA2f { get; set; }
    public string codeA2f { get; set; }
    public int? userIdForA2f { get; set; }
    public string emailForA2f { get; set; }
    
    private Action? _refreshUI;
    private NavigationManager _nav;

    public ConnexionViewModel(
        IServiceConnexion connexionService, 
        ICompteService compteservice,
        ITokenEmailService tokenEmailService)
    {
        _connexionService = connexionService;
        _compteService = compteservice;
        _tokenEmailService = tokenEmailService;
        messageErreur = string.Empty;
        isLoading = false;
        requiresA2f = false;
        mustReactivateA2f = false;
        codeA2f = string.Empty;
    }

    public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;
    }

    public async Task ConnexionUtilisateur()
    {
        // Validation côté client
        if (string.IsNullOrWhiteSpace(emailUtilisateur))
        {
            messageErreur = "Veuillez saisir votre email";
            _refreshUI?.Invoke();
            return;
        }

        if (string.IsNullOrWhiteSpace(motDePasseUtilisateur))
        {
            messageErreur = "Veuillez saisir votre mot de passe";
            _refreshUI?.Invoke();
            return;
        }

        isLoading = true;
        messageErreur = string.Empty;
        _refreshUI?.Invoke();

        try
        {
            var req = new LoginRequest()
            {
                Email = emailUtilisateur,
                MotDePasse = motDePasseUtilisateur,
            };
            
            var result = await _connexionService.LoginUser(req);

            isLoading = false;

            // Vérifier si A2F est requis (code 202)
            if (result.StatusCode == HttpStatusCode.Accepted)
            {
                // Récupérer les infos depuis la réponse
                A2fResponse content = await result.Content.ReadFromJsonAsync<A2fResponse>();

                requiresA2f = true;
                mustReactivateA2f = content?.MustReactivate ?? false;
                userIdForA2f = content?.UserId;
                emailForA2f = content?.Email ?? emailUtilisateur;
                
                messageErreur = string.Empty;
                _refreshUI?.Invoke();
            }

            switch (result.StatusCode)
            {
                case HttpStatusCode.OK:
                    await Task.Delay(100);

                    CompteDetailDTO moi = await _compteService.GetMe();
                    if (moi.IdTypeCompte == 3)
                    {
                        _nav.NavigateTo("/admin");
                        break;
                    }

                    _nav.NavigateTo("/compte", forceLoad: true);
                    break;

                case HttpStatusCode.Unauthorized:
                    messageErreur = "Email ou mot de passe incorrect";
                    _refreshUI?.Invoke();
                    break;

                case HttpStatusCode.BadRequest:
                    messageErreur = "Données invalides";
                    _refreshUI?.Invoke();
                    break;

                default:
                    _refreshUI?.Invoke();
                    break;
            }
        }
        catch (Exception ex)
        {
            isLoading = false;
            messageErreur = "Erreur de connexion au serveur";
            Console.WriteLine($"Erreur ConnexionUtilisateur: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }

    public async Task ValiderCodeA2f()
    {
        if (string.IsNullOrWhiteSpace(codeA2f))
        {
            messageErreur = "Veuillez saisir le code reçu par email";
            _refreshUI?.Invoke();
            return;
        }

        isLoading = true;
        messageErreur = string.Empty;
        _refreshUI?.Invoke();

        try
        {
            // Vérifier le code
            var verifDto = new TokenEmailVerifDTO
            {
                Email = emailForA2f,
                Code = codeA2f,
                TypeToken = mustReactivateA2f ? "A2F_ACTIVATION" : "A2F_CONNEXION"
            };

            bool codeValide = await _tokenEmailService.VerifierCode(verifDto);

            if (!codeValide)
            {
                messageErreur = "Code invalide ou expiré";
                isLoading = false;
                _refreshUI?.Invoke();
                return;
            }

            // Valider la connexion A2F via l'API
            var result = await _connexionService.ValidateA2fLogin(verifDto);

            isLoading = false;

            if (result.StatusCode == HttpStatusCode.OK)
            {
                await Task.Delay(100);
                
                CompteDetailDTO moi = await _compteService.GetMe();
                if (moi.IdTypeCompte == 3)
                {
                    _nav.NavigateTo("/admin");
                }
                else
                {
                    _nav.NavigateTo("/compte", forceLoad: true);
                }
            }
            else
            {
                messageErreur = "Erreur lors de la validation du code";
                _refreshUI?.Invoke();
            }
        }
        catch (Exception ex)
        {
            isLoading = false;
            messageErreur = "Erreur de connexion au serveur";
            Console.WriteLine($"Erreur ValiderCodeA2f: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }

    public async Task RenvoyerCodeA2f()
    {
        isLoading = true;
        messageErreur = string.Empty;
        _refreshUI?.Invoke();

        try
        {
            var dto = new TokenEmailCreateDTO
            {
                IdCompte = userIdForA2f ?? 0,
                Email = emailForA2f,
                TypeToken = mustReactivateA2f ? "A2F_ACTIVATION" : "A2F_CONNEXION"
            };

            bool success = await _tokenEmailService.EnvoyerToken(dto);

            isLoading = false;

            if (success)
            {
                messageErreur = "Un nouveau code a été envoyé";
            }
            else
            {
                messageErreur = "Erreur lors de l'envoi du code";
            }

            _refreshUI?.Invoke();
        }
        catch (Exception ex)
        {
            isLoading = false;
            messageErreur = "Erreur lors de l'envoi du code";
            Console.WriteLine($"Erreur RenvoyerCodeA2f: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }
    
    public void SetNavigation(NavigationManager nav)
    {
        _nav = nav;
    }

    public async Task DeconnexionUtilisateur()
    {
        try
        {
            await _connexionService.LogOutUser();
            await Task.Delay(100);
            _nav.NavigateTo("/connexion", forceLoad: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur DeconnexionUtilisateur: {ex.Message}");
            _nav.NavigateTo("/connexion", forceLoad: true);
        }
    }
    
    public async Task ConnecterAvecGoogle()
    {
        try
        {
            isLoading = true;
            messageErreur = string.Empty;
            _refreshUI?.Invoke();

            var response = await _connexionService.GoogleLogin();

            if (response != null && !string.IsNullOrEmpty(response.Url))
            {
                _nav.NavigateTo(response.Url, forceLoad: true);
            }
            else
            {
                messageErreur = "Erreur lors de la génération de l'URL Google";
                isLoading = false;
                _refreshUI?.Invoke();
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de la connexion avec Google";
            isLoading = false;
            Console.WriteLine($"Erreur ConnecterAvecGoogle: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }
    
    public void Reset()
    {
        emailUtilisateur = string.Empty;
        motDePasseUtilisateur = string.Empty;
        messageErreur = string.Empty;
        isLoading = false;
        requiresA2f = false;
        mustReactivateA2f = false;
        codeA2f = string.Empty;
        userIdForA2f = null;
        emailForA2f = string.Empty;
    }
}