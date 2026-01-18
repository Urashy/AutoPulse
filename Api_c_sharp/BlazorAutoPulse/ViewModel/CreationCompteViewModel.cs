using System.Text.RegularExpressions;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class CreationCompteViewModel
{
    private readonly ICompteService _compteService;
    private readonly IServiceConnexion _connexionService;
    private readonly ITokenEmailService _tokenEmailService;
    private readonly NotificationService _notificationService;
    private readonly IA2fService _a2fService;

    public bool pro = false;
    
    public CompteCreateDTO compte { get; set; }
    public string motDePasse { get; set; }
    
    public bool memeMotDePasse { get; set; }
    public string messageErreur { get; set; }
    
    // Nouveaux champs pour A2F optionnel
    public bool afficherA2f { get; set; }
    public bool activerA2f { get; set; }
    public string codeA2f { get; set; }
    public bool codeA2fEnvoye { get; set; }
    public bool isLoadingA2f { get; set; }
    
    private Action? _refreshUI;
    private NavigationManager _nav;
    public bool showPopUp { get; set; }
    public int seconds { get; set; }
    
    private System.Threading.Timer? _countdownTimer;

    public CreationCompteViewModel(
        ICompteService compteService, 
        IServiceConnexion connexionService,
        ITokenEmailService tokenEmailService,
        NotificationService notificationService,
        IA2fService a2fService)
    {
        _compteService  = compteService;
        _connexionService = connexionService;
        _tokenEmailService = tokenEmailService;
        _notificationService = notificationService;
        _a2fService = a2fService;
        
        compte = new CompteCreateDTO();
        compte.IdTypeCompte = 1;
        compte.DateNaissance = new DateTime(1900, 1, 1);
        compte.EstSuspendu = false;
        memeMotDePasse = true;
        showPopUp = false;
        seconds = 5;
        afficherA2f = false;
        activerA2f = false;
        codeA2fEnvoye = false;
        isLoadingA2f = false;
    }
    
    public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
    {
        Reset();
        _refreshUI = refreshUI;
        _nav = nav;
    }
    
    public void Reset()
    {
        compte = new CompteCreateDTO();
        compte.IdTypeCompte = 1;
        compte.DateNaissance = new DateTime(1900, 1, 1);
        compte.EstSuspendu = false;
        
        motDePasse = string.Empty;
        memeMotDePasse = true;
        messageErreur = null;
        
        pro = false;
        afficherA2f = false;
        activerA2f = false;
        codeA2f = string.Empty;
        codeA2fEnvoye = false;
        isLoadingA2f = false;
        showPopUp = false;
        seconds = 5;
        
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }

    /// <summary>
    /// ✅ Vérifie en temps réel si les mots de passe correspondent
    /// </summary>
    public void VerifierCorrespondanceMotDePasse()
    {
        // Ne vérifie que si les deux champs ont du contenu
        if (!string.IsNullOrEmpty(compte.MotDePasse) && !string.IsNullOrEmpty(motDePasse))
        {
            memeMotDePasse = compte.MotDePasse == motDePasse;
        }
        else
        {
            // Si un des champs est vide, on considère qu'il n'y a pas d'erreur à afficher
            memeMotDePasse = true;
        }
    }

    public async Task CreateCompteAsync()
    {
        messageErreur = null;

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

        if (string.IsNullOrWhiteSpace(compte.MotDePasse))
        {
            messageErreur = "Le mot de passe est requis";
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
        
        if (compte.DateNaissance == new DateTime(1900, 1, 1))
        {
            messageErreur = "La date de naissance est requise"; 
            _refreshUI?.Invoke();
            return;
        }

        try
        {
            var result = await _compteService.PostWithErrorHandlingAsync(compte);

            if (result.Success)
            {
                _notificationService.ShowSuccess(
                    "Compte créé !",
                    "Un email de vérification a été envoyé à votre adresse. Veuillez vérifier votre boîte de réception."
                );
            
                showPopUp = true;
                seconds = 5;
                _refreshUI?.Invoke();
            
                await StartCountdownAndRedirect();
            }
            else
            {
                messageErreur = result.ErrorMessage;
                _refreshUI?.Invoke();
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Une erreur inattendue s'est produite";
            Console.WriteLine($"Exception CreateCompteAsync: {ex.Message}");
            _refreshUI?.Invoke();
        }
    }

    private async Task StartCountdownAndRedirect()
    {
        _countdownTimer = new Timer(async _ =>
        {
            seconds--;
            _refreshUI?.Invoke();

            if (seconds <= 0)
            {
                _countdownTimer?.Dispose();
                _countdownTimer = null;
                _nav?.NavigateTo("/connexion");
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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
    
    public void Dispose()
    {
        _countdownTimer?.Dispose();
        _countdownTimer = null;
    }
}