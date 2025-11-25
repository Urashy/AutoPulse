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
    private HttpStatusCode httpCode;
    
    private Action? _refreshUI;
    private NavigationManager _nav;

    public ConnexionViewModel(IServiceConnexion connexionService)
    {
        _connexionService  = connexionService;
    }

    public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;
    }

    public async Task ConnexionUtilisateur()
    {
        var req = new LoginRequest()
        {
            Email = emailUtilisateur,
            MotDePasse = motDePasseUtilisateur,
        };
        
        httpCode = await _connexionService.LoginUser(req);
        if (httpCode == HttpStatusCode.OK)
        {
            await Task.Delay(100);
            _nav.NavigateTo("Compte");
        }
        else
        {
            Console.WriteLine("Erreur lors de la connexion de l'utilisateur");
        }
    }
}