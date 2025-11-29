using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class OubliMdpViewModel
{
    private readonly IReinitialiseMdp _reinitMdpService;
    private readonly ICompteService _compteService;
    
    public ReinitialisationMdp ReinitialisationMdp { get; set; }
    public string messageErreur = "";
    public string messageSucces = "";
    public bool isLoading = false;
    public bool envoiDemande = false;
    public bool codeValide = false;
    public int tempsRestant = 60;
    public System.Threading.Timer? timer;
    
    // Nouveaux champs pour le changement de mot de passe
    public string nouveauMotDePasse = "";
    public string confirmationMotDePasse = "";
    public bool motDePasseValide = true;
    
    public Action? _refreshUI;
    public NavigationManager _nav;

    public OubliMdpViewModel(IReinitialiseMdp reinitMdpService, ICompteService compteService)
    {
        _reinitMdpService = reinitMdpService;
        _compteService = compteService;
    }

    public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;

        ReinitialisationMdp = new ReinitialisationMdp()
        {
            Email = "",
            Code = null
        };
    }

    public async Task EnvoyerCodeReinitialisation()
    {
        messageErreur = "";

        if (string.IsNullOrWhiteSpace(ReinitialisationMdp.Email))
        {
            messageErreur = "Veuillez entrer une adresse email.";
            return;
        }

        isLoading = true;
        _refreshUI?.Invoke();

        Compte compte = await _compteService.GetByNameAsync(ReinitialisationMdp.Email);
        ReinitialisationMdp.IdCompte = compte.IdCompte;
        await _reinitMdpService.CreateAsync(ReinitialisationMdp);

        envoiDemande = true;
        isLoading = false;
        DemarrerCompteur();
        _refreshUI?.Invoke();
    }

    public async Task ValiderCode()
    {
        messageErreur = "";

        if (string.IsNullOrWhiteSpace(ReinitialisationMdp.Code))
        {
            messageErreur = "Veuillez entrer le code de vérification.";
            return;
        }

        isLoading = true;
        _refreshUI?.Invoke();

        bool codeAccepte = await _reinitMdpService.VerifCode(ReinitialisationMdp);

        if (codeAccepte)
        {
            codeValide = true;
            timer?.Dispose();
        }
        else
        {
            messageErreur = "Code incorrect. Veuillez réessayer.";
        }

        isLoading = false;
        _refreshUI?.Invoke();
    }

    public async Task ChangerMotDePasse()
    {
        messageErreur = "";
        messageSucces = "";
        motDePasseValide = true;

        if (string.IsNullOrWhiteSpace(nouveauMotDePasse))
        {
            messageErreur = "Veuillez entrer un nouveau mot de passe.";
            motDePasseValide = false;
            _refreshUI?.Invoke();
            return;
        }

        if (string.IsNullOrWhiteSpace(confirmationMotDePasse))
        {
            messageErreur = "Veuillez confirmer votre mot de passe.";
            motDePasseValide = false;
            _refreshUI?.Invoke();
            return;
        }

        if (nouveauMotDePasse != confirmationMotDePasse)
        {
            messageErreur = "Les mots de passe ne correspondent pas.";
            motDePasseValide = false;
            _refreshUI?.Invoke();
            return;
        }

        if (nouveauMotDePasse.Length < 8)
        {
            messageErreur = "Le mot de passe doit contenir au moins 8 caractères.";
            motDePasseValide = false;
            _refreshUI?.Invoke();
            return;
        }

        isLoading = true;
        _refreshUI?.Invoke();

        try
        {
            ChangementMdp nouveauMdp = new ChangementMdp
            {
                IdCompte = ReinitialisationMdp.IdCompte,
                MotDePasse = nouveauMotDePasse
            };
            _compteService.ChangementMdp(nouveauMdp);
            await _reinitMdpService.DeleteByNameAsync(ReinitialisationMdp.Code);
            
            messageSucces = "Mot de passe changé avec succès ! Redirection...";
            _refreshUI?.Invoke();
            
            await Task.Delay(2000);
            _nav.NavigateTo("/connexion");
        }
        catch (Exception ex)
        {
            messageErreur = "Une erreur est survenue lors du changement de mot de passe.";
            Console.WriteLine($"Erreur: {ex.Message}");
        }

        isLoading = false;
        _refreshUI?.Invoke();
    }

    public async Task RenvoyerCode()
    {
        messageErreur = "";
        tempsRestant = 60;
        
        isLoading = true;
        _refreshUI?.Invoke();

        await _reinitMdpService.CreateAsync(ReinitialisationMdp);

        isLoading = false;
        DemarrerCompteur();
        _refreshUI?.Invoke();
    }

    public void DemarrerCompteur()
    {
        timer?.Dispose();
        tempsRestant = 60;

        timer = new System.Threading.Timer(_ =>
        {
            if (tempsRestant > 0)
            {
                tempsRestant--;
                _refreshUI?.Invoke();
            }
            else
            {
                timer?.Dispose();
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }
}