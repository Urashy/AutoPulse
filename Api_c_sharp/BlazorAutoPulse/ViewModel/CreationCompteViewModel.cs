using System.Text.RegularExpressions;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class CreationCompteViewModel
{
    private readonly IService<Compte> _compteService;
    private readonly IServiceConnexion _connexionService;

    public bool pro = false;
    
    public Compte compte { get; set; }
    public string motDePasse { get; set; }
    
    public bool memeMotDePasse { get; set; }
    public string messageErreur { get; set; }
    
    private Action? _refreshUI;
    private NavigationManager _nav;
    public bool showPopUp { get; set; }
    public int seconds { get; set; }

    public CreationCompteViewModel(IService<Compte> compteService, IServiceConnexion connexionService)
    {
        _compteService  = compteService;
        _connexionService = connexionService;
        compte = new Compte();
        compte.DateNaissance = new DateTime(2000, 1, 1);
        memeMotDePasse = true;
        showPopUp = false;
        seconds = 3;
    }
    
    public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;
    }

    public async Task CreateCompteAsync()
    {
        // Reset message d'erreur
        messageErreur = null;

        // Validation côté client
        if (string.IsNullOrWhiteSpace(compte.Pseudo))
        {
            messageErreur = "Le pseudo est requis";
            _refreshUI?.Invoke();
            return;
        }

        if (string.IsNullOrWhiteSpace(compte.Email))
        {
            messageErreur = "L'email est requis";
            _refreshUI?.Invoke();
            return;
        }

        if (compte.MotDePasse != motDePasse)
        {
            messageErreur = "Les mots de passe ne correspondent pas";
            memeMotDePasse = false;
            _refreshUI?.Invoke();
            return;
        }

        try
        {
            // ✅ Utiliser la nouvelle méthode avec gestion d'erreur
            var result = await _compteService.PostWithErrorHandlingAsync(compte);

            if (result.Success)
            {
                // Succès - afficher popup et rediriger
                showPopUp = true;
                _refreshUI?.Invoke();
                
                await Task.Delay(3000);
                _nav?.NavigateTo("/connexion");
            }
            else
            {
                // ✅ Afficher l'erreur retournée par le backend
                messageErreur = result.ErrorMessage;
                
                // Log pour debug
                Console.WriteLine($"Erreur création compte: {result.ErrorMessage}");
                
                if (result.ValidationErrors != null)
                {
                    foreach (var error in result.ValidationErrors)
                    {
                        Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value)}");
                    }
                }
                
                _refreshUI?.Invoke();
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Une erreur inattendue s'est produite lors de la création du compte";
            Console.WriteLine($"Exception CreateCompteAsync: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }
    
    public async Task ConnecterAvecGoogle()
    {
        try
        {
            var response = await _connexionService.GoogleLogin();

            if (response != null && !string.IsNullOrEmpty(response.Url))
            {
                _nav.NavigateTo(response.Url, forceLoad: true);
            }
            else
            {
                messageErreur = "Erreur lors de la génération de l'URL Google";
                _refreshUI?.Invoke();
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de la connexion avec Google";
            Console.WriteLine($"Erreur ConnecterAvecGoogle: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }

    public async Task ReloadPage()
    {
        pro = !pro;
        _refreshUI?.Invoke();
    }
}