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
    private string action = "";
    
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
        action = "Connexion";
        VerifCode();
    }

    public async Task DeconnexionUtilisateur()
    {
        httpCode = await _connexionService.LogOutUser();
        action = "Deconnexion";
        VerifCode();
    }

    private async void VerifCode()
    {
        if (httpCode == HttpStatusCode.OK)
        {
            await Task.Delay(100);
            if (action == "Connexion")
            {
                _nav.NavigateTo("compte");
            }
            else
            {
                _nav.NavigateTo("connexion");
            }
        }
        else
        {
            Console.WriteLine("Erreur lors de la connexion de l'utilisateur");
        }
        action = "";
    }
    
    public void Reset()
    {
        emailUtilisateur = string.Empty;
        motDePasseUtilisateur = string.Empty;
        httpCode = 0;
        action = "";
    }
}