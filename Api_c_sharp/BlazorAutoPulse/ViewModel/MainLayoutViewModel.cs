using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class MainLayoutViewModel
    {
        private readonly ICompteService _compteService;
        private readonly IImageService _imageService;

        public bool IsConnected { get; private set; }
        public bool IsAdmin { get; private set; }
        public string ImageSource { get; private set; } = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public MainLayoutViewModel(ICompteService compteService, IImageService imageService)
        {
            _compteService = compteService;
            _imageService = imageService;
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            await CheckConnexion();
        }

        private async Task CheckConnexion()
        {
            try
            {
                var compte = await _compteService.GetMe();
                IsConnected = compte != null;

                if (IsConnected)
                {
                    await LoadProfileImage(compte.IdCompte);
                }
            }
            catch
            {
                IsConnected = false;
            }

            _refreshUI?.Invoke();
        }

        private async Task LoadProfileImage(int idCompte)
        {
            try
            {
                var img = await _imageService.GetImageProfil(idCompte);

                if (img != null && img.Fichier != null && img.Fichier.Length > 0)
                {
                    var base64 = Convert.ToBase64String(img.Fichier);
                    ImageSource = $"data:image/jpeg;base64,{base64}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement image profil: {ex.Message}");
            }
        }

        public void NavigateToProfile()
        {
            _nav?.NavigateTo("/compte");
        }
    }
}