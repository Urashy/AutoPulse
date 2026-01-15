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
        compte.DateNaissance = new DateTime(2000, 1, 1);
        compte.EstSuspendu = false;
        memeMotDePasse = true;
        showPopUp = false;
        seconds = 3;
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
        compte.DateNaissance = new DateTime(2000, 1, 1);
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
        seconds = 3;
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

        if (compte.MotDePasse != motDePasse)
        {
            messageErreur = "Les mots de passe ne correspondent pas";
            memeMotDePasse = false;
            _refreshUI?.Invoke();
            return;
        }

        try
        {
            var result = await _compteService.PostWithErrorHandlingAsync(compte);

            if (result.Success)
            {
                // ✅ Afficher le message de succès avec redirection
                _notificationService.ShowSuccess(
                    "Compte créé !",
                    "Un email de vérification a été envoyé à votre adresse. Veuillez vérifier votre boîte de réception."
                );
            
                showPopUp = true;
                _refreshUI?.Invoke();
            
                await Task.Delay(5000);
                _nav?.NavigateTo("/connexion");
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

    public async Task SkipA2f()
    {
        showPopUp = true;
        _refreshUI?.Invoke();
        
        await Task.Delay(3000);
        _nav?.NavigateTo("/connexion");
    }

    public async Task EnvoyerCodeA2f()
    {
        isLoadingA2f = true;
        messageErreur = null;
        _refreshUI?.Invoke();

        try
        {
            var compteCreated = await _compteService.GetByNameAsync(compte.Email);

            var dto = new TokenEmailCreateDTO
            {
                IdCompte = compteCreated.IdCompte,
                Email = compte.Email,
                TypeToken = "A2F_ACTIVATION"
            };

            bool success = await _tokenEmailService.EnvoyerToken(dto);

            if (success)
            {
                codeA2fEnvoye = true;
                messageErreur = null;
            }
            else
            {
                messageErreur = "Erreur lors de l'envoi du code";
            }
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de l'envoi du code";
            Console.WriteLine($"Erreur EnvoyerCodeA2f: {ex.Message}");
        }
        finally
        {
            isLoadingA2f = false;
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

        isLoadingA2f = true;
        messageErreur = null;
        _refreshUI?.Invoke();

        try
        {
            // Vérifier le code
            var verifDto = new TokenEmailVerifDTO
            {
                Email = compte.Email,
                Code = codeA2f,
                TypeToken = "A2F_ACTIVATION"
            };

            bool codeValide = await _tokenEmailService.VerifierCode(verifDto);

            if (!codeValide)
            {
                messageErreur = "Code invalide ou expiré";
                isLoadingA2f = false;
                _refreshUI?.Invoke();
                return;
            }

            // Activer l'A2F
            var compteCreated = await _compteService.GetByNameAsync(compte.Email);

            var activationDto = new A2fActivationDTO
            {
                IdCompte = compteCreated.IdCompte
            };

            bool success = await _a2fService.ActiverA2f(activationDto);

            if (success)
            {
                _notificationService.ShowSuccess(
                    "A2F activé",
                    "L'authentification à deux facteurs a été activée avec succès"
                );
            }
            else
            {
                _notificationService.ShowSuccess(
                    "Erreur A2F",
                    "Erreur lors de l'activation de l'A2F"
                );
            }

            showPopUp = true;
            _refreshUI?.Invoke();
            
            await Task.Delay(3000);
            _nav?.NavigateTo("/connexion");
        }
        catch (Exception ex)
        {
            messageErreur = "Erreur lors de la validation du code";
            Console.WriteLine($"Erreur ValiderCodeA2f: {ex.Message}");
        }
        finally
        {
            isLoadingA2f = false;
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