using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class OubliMdpViewModel
{
    
    private readonly IReinitialiseMdp _reinitMdpService;
    
    public ReinitialisationMdp ReinitialisationMdp { get; set; }
    public string messageErreur = "";
    public bool isLoading = false;
    public bool envoiDemande = false;
    public int tempsRestant = 60;
    public System.Threading.Timer? timer;
    
    public Action? _refreshUI;
    public NavigationManager _nav;

    public OubliMdpViewModel(IReinitialiseMdp reinitMdpService)
    {
        _reinitMdpService = reinitMdpService;
    }

    public async Task InitializeAsync(Action refreshUI,  NavigationManager nav)
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
            _reinitMdpService.DeleteByNameAsync(ReinitialisationMdp.Code);
            _nav.NavigateTo("/nouveau-mdp");
        }
        else
        {
            messageErreur = "Code incorrect. Veuillez réessayer.";
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