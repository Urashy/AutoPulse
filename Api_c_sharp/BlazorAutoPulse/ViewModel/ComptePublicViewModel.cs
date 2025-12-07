using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class ComptePublicViewModel
{
    // Service
    private readonly ICompteService _compteService;
    private readonly NotificationService _notificationService;
    private readonly IImageService _imageService;
    
    // Modele
    public CompteProfilPublicDTO compte { get; set; }
    
    public bool isLoading = true;
    
    // Menu
    public bool IsOptionsMenuOpen { get; private set; } = false;
    public string? infoOption { get; private set; } = null;
    
    // Image
    private string mimeType = "data:image/jpeg;base64,";
    public string imageSource;
    public int idImage;
    
    // Section
    public string activeSection = "annonces";
    public IEnumerable<AnnonceDTO> annonces;
    public IEnumerable<AvisListDTO> avis;
    
    // Variable d'action
    private Action? _refreshUI;
    public NavigationManager _nav { get; set; }

    public ComptePublicViewModel(
        ICompteService compteService,
        NotificationService notificationService,
        IImageService imageService)
    {
        _compteService = compteService;
        _notificationService = notificationService;
        _imageService = imageService;
    }
    public async Task InitializeAsync(int idCompte, Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;

        try
        {
            compte = await _compteService.GetComptePublicById(idCompte);
            await GetImageProfil(compte.IdCompte);
            isLoading = false;
        }
        catch
        {
            RedirectionAccueil();
        }
        _refreshUI?.Invoke();
    }
    
    public void ToggleOptionsMenu()
    {
        IsOptionsMenuOpen = !IsOptionsMenuOpen;
        if (!IsOptionsMenuOpen)
        {
            infoOption = null;
        }
        _refreshUI?.Invoke();
    }
    
    public void SetInfoOption(string? info)
    {
        infoOption = info;
        _refreshUI?.Invoke();
    }
    
    private async Task GetImageProfil(int id)
    {
        try
        {
            Image? img = await _imageService.GetImageProfil(id);

            imageSource = "";
            if (img != null && img.Fichier != null && img.Fichier.Length > 0)
            {
                idImage = img.IdImage;
                var base64 = Convert.ToBase64String(img.Fichier);
                imageSource = $"{mimeType}{base64}";
            }
            else
            {
                imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
            }

            _refreshUI?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'affichage de l'image : {ex.Message}");
            imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
            _refreshUI?.Invoke();
        }
    }

    public async Task EnvoyerMessage()
    {
        
    }

    public async Task SignalerUtilisateur()
    {
        
    }
    
    public void SetActiveSection(string section)
    {
        activeSection = section;
    }

    public async Task RedirectionAccueil()
    {
        _notificationService.ShowError(
            "Utilisateur introuvable",
            "Vous avez été redirigé vers l'accueil");
        _nav.NavigateTo("/");
    }
}