using System.Diagnostics;
using System.Net;
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
    private readonly IBloqueService _bloqueService;
    private readonly IAnnonceService _annonceService;
    private readonly NotificationService _notificationService;
    private readonly IImageService _imageService;
    
    // Modele
    public CompteProfilPublicDTO comptePublic { get; set; }
    public CompteDetailDTO compte  { get; set; }
    
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
    
    // Bloquer
    public bool EstBloque { get; set; }
    
    // Non connecter ou compte n'existe pas
    private string titleError { get; set; }
    private string messageError { get; set; }
    
    // Variable d'action
    private Action? _refreshUI;
    public NavigationManager _nav { get; set; }

    public ComptePublicViewModel(
        ICompteService compteService,
        IBloqueService bloqueService,
        NotificationService notificationService,
        IAnnonceService annonceService,
        IImageService imageService)
    {
        _compteService = compteService;
        _bloqueService = bloqueService;
        _notificationService = notificationService;
        _annonceService = annonceService;
        _imageService = imageService;
    }
    public async Task InitializeAsync(int idCompte, Action refreshUI, NavigationManager nav)
    {
        _refreshUI = refreshUI;
        _nav = nav;

        try
        {
            comptePublic = await _compteService.GetComptePublicById(idCompte);
            await GetImageProfil(comptePublic.IdCompte);

            compte = await _compteService.GetMe();
            annonces = await _annonceService.GetByCompteID(comptePublic.IdCompte);
            
            await ABloquer(false);
            if (EstBloque)
            {
                _notificationService.ShowInfo(
                    "Bloquer",
                    $"Vous avez été rediriger vers l'accueil car {comptePublic.Pseudo} vous à bloqué");
                _nav.NavigateTo("/");
                return;
            }
            isLoading = false;
        }
        catch (HttpRequestException ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    titleError = "Non connecté";
                    messageError = "Vous devez être connecté pour accéder à un profil";
                    break;
                case HttpStatusCode.NotFound:
                    titleError = "Utilisateur introuvable";
                    messageError = "Vous avez été redirigé vers l'accueil";
                    break;
            }
            
            RedirectionAccueil();
        }

        ABloquer(true);
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
            ImageDTO? img = await _imageService.GetImageProfil(id);

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

    public async Task BloquerUtilisateur()
    {
        BloqueDTO bloque = new BloqueDTO()
        {
            IdBloque = comptePublic.IdCompte,
            IdBloquant = compte.IdCompte
        };

        try
        {
            await _bloqueService.PostWithErrorHandlingAsync(bloque);
            _notificationService.ShowSuccess(
                "Bloquer",
                "L'utilisateur à bien été bloquer");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError(
                "Bloquer",
                $"L'utilisateur n'a pas pu être bloquer ({ex.Message})");
        }
        
        await ABloquer(true);
        _refreshUI?.Invoke();
    }

    public async Task DebloquerUtilisateur()
    {
        try
        {
            await _bloqueService.DeleteBloque(compte.IdCompte, comptePublic.IdCompte);
            _notificationService.ShowSuccess(
                "Bloquer",
                "L'utilisateur à été débloquer");
        }
        catch (Exception e)
        {
            _notificationService.ShowError(
                "Erreur bloquer",
                $"{e.Message}");
        }

        await ABloquer(true);
        _refreshUI?.Invoke();
    }

    public async Task ABloquer(bool premierBloque)
    {
        EstBloque = await _bloqueService.ABloque(compte.IdCompte,  comptePublic.IdCompte, premierBloque);
        _refreshUI?.Invoke();
    }
    
    public void SetActiveSection(string section)
    {
        activeSection = section;
    }

    public async Task RedirectionAccueil()
    {
        if (titleError != null && messageError != null)
        {
            _notificationService.ShowError(
                titleError,
                messageError);
        }
        _nav.NavigateTo("/");
    }
}