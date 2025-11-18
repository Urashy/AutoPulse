using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;

namespace BlazorAutoPulse.ViewModel;

public class ConnexionViewModel
{
    private readonly IServiceConnexion _connexionService;
    
    public string emailUtilisateur { get; set; }
    public string motDePasseUtilisateur { get; set; }
    
    public Compte User { get; set; }
    
    private Action? _refreshUI;

    public ConnexionViewModel(IServiceConnexion connexionService)
    {
        _connexionService  = connexionService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
    }

    public async Task ConnexionUtilisateur()
    {
        var req = new LoginRequest()
        {
            Email = emailUtilisateur,
            MotDePasse = motDePasseUtilisateur,
        };

        User = await _connexionService.LoginUser(req);
        await Task.Delay(100);
        _refreshUI?.Invoke();
    }
}