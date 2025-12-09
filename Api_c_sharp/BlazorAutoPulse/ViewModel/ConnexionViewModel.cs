using System.Net;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class ConnexionViewModel
{
    private readonly IServiceConnexion _connexionService;
    
    public string emailUtilisateur { get; set; }
    public string motDePasseUtilisateur { get; set; }
    public string messageErreur { get; set; }
    public bool isLoading { get; set; }
    
    private Action? _refreshUI;
    private NavigationManager _nav;

    public ConnexionViewModel(IServiceConnexion connexionService)
    {
        _connexionService = connexionService;
        messageErreur = string.Empty;
        isLoading = false;
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
            
            var httpCode = await _connexionService.LoginUser(req);

            isLoading = false;

            switch (httpCode)
            {
                case HttpStatusCode.OK:
                    // Succès - redirection
                    await Task.Delay(100);
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
                    messageErreur = "Erreur de connexion. Veuillez réessayer.";
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

            // Appel à l'API pour obtenir l'URL de redirection Google
            var response = await _connexionService.GoogleLogin();

            if (response != null && !string.IsNullOrEmpty(response.Url))
            {
                // Redirige vers Google (forceLoad = true pour charger la page externe)
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
    }
}