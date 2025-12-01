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
        memeMotDePasse = true;
        
        compte.IdTypeCompte = (pro) ? 1 : 2;
        try
        {
            if (compte.MotDePasse != motDePasse)
            {
                memeMotDePasse = false;
                messageErreur = "MotDePasse différent";
            }
            
            _refreshUI?.Invoke();
            if (!memeMotDePasse)
            {
                return;
            }
            
            var createdCompte = await _compteService.CreateAsync(compte);
            showPopUp = true;
            while (seconds > 0)
            {
                await Task.Delay(1000);
                seconds--;
                _refreshUI?.Invoke();
            }
            _nav.NavigateTo("/connexion");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Erreur lors de la création : " + ex.Message);
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