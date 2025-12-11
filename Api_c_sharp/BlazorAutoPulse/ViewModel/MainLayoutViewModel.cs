using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class MainLayoutViewModel : IDisposable
    {
        private readonly ICompteService _compteService;
        private readonly IImageService _imageService;
        private readonly IAnnonceService _annonceService;
        private readonly ISignalRService _signalRService;
        private ConversationStateService _conversationStateService;

        public bool IsConnected { get; private set; }
        public bool IsAdmin { get; private set; }
        public string ImageSource { get; private set; } = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";

        public int unreadCount = 0;
        public int notificationsCount = 0;
        
        private Action? _refreshUI;
        private NavigationManager? _nav;
        private List<int> _favorisAnnonceIds = new();

        public MainLayoutViewModel(
            ICompteService compteService, 
            IImageService imageService,
            IAnnonceService annonceService,
            ISignalRService signalRService)
        {
            _compteService = compteService;
            _imageService = imageService;
            _annonceService = annonceService;
            _signalRService = signalRService;
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav, ConversationStateService conversationStateService)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            _conversationStateService = conversationStateService;

            var uri = nav.Uri.ToLower();
            bool isPublicPage = uri.Contains("/connexion") ||
                                uri.Contains("/creationcompte") ||
                                uri.Contains("/oublimdp");

            if (!isPublicPage)
            {
                await CheckConnexion();
            }
        }

        private async Task CheckConnexion()
        {
            try
            {
                CompteDetailDTO compte = await _compteService.GetMe();
                IsConnected = compte != null;

                if (IsConnected)
                {
                    IsAdmin = compte.TypeCompte == "Administrateur";
                    await LoadProfileImage(compte.IdCompte);
                    
                    // Initialiser les conversations
                    await _conversationStateService.InitializeAsync();
                    unreadCount = _conversationStateService.GetTotalUnreadCount();
                    _conversationStateService.OnStateChanged += UpdateUnreadCount;
                    
                    // ✅ Rejoindre les hubs SignalR pour les favoris
                    await JoinFavorisHubs(compte.IdCompte);
                    
                    // ✅ S'abonner aux notifications de baisse de prix
                    _signalRService.OnPriceDropReceived += HandlePriceDropNotification;
                }
            }
            catch
            {
                IsConnected = false;
            }

            _refreshUI?.Invoke();
        }

        private async Task JoinFavorisHubs(int idCompte)
        {
            try
            {
                var annoncesFavoris = await _annonceService.GetAnnoncesFavoritesByCompteId(idCompte);
                _favorisAnnonceIds = annoncesFavoris.Select(a => a.IdAnnonce).ToList();
                
                foreach (var idAnnonce in _favorisAnnonceIds)
                {
                    await _signalRService.JoinFavorisAnnonce(idAnnonce);
                }
                
                Console.WriteLine($"✅ Rejoint {_favorisAnnonceIds.Count} annonces favorites");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de la jointure aux hubs favoris: {ex.Message}");
            }
        }

        private void HandlePriceDropNotification(PriceDropNotification notif)
        {
            Console.WriteLine($"🔔 Baisse de prix détectée: {notif.AnnonceLibelle} ({notif.OldPrice}€ → {notif.NewPrice}€)");
            notificationsCount++;
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

        private void UpdateUnreadCount()
        {
            unreadCount = _conversationStateService.GetTotalUnreadCount();
            Console.WriteLine($"🔔 Badge mis à jour: {unreadCount} messages non lus");
            _refreshUI?.Invoke();
        }

        public void NavigateToProfile()
        {
            _nav?.NavigateTo("/compte");
        }

        public void NavigateToNotifications()
        {
            notificationsCount = 0;
            _nav?.NavigateTo("/notifications");
            _refreshUI?.Invoke();
        }

        public void Dispose()
        {
            if (_conversationStateService != null)
            {
                _conversationStateService.OnStateChanged -= UpdateUnreadCount;
            }
            
            if (_signalRService != null)
            {
                _signalRService.OnPriceDropReceived -= HandlePriceDropNotification;
            }
        }
    }
}