using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Authentification;
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
        private readonly IPlainteService _plainteService;
        private readonly ISignalementService _signalementService;
        private readonly IServiceConnexion _connexionService;
        private readonly IAnnonceService _annonceService;
        private readonly ISignalRService _signalRService;
        private readonly INotificationService _notificationService;
        private readonly NotificationService _notificationToastService;
        private ConversationStateService _conversationStateService;
        private readonly FavoriStateService _favorisStateService;
        
        public bool IsConnected { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsAccountSuspended { get; private set; }
        public string ImageSource { get; private set; } = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";

        public int unreadCount = 0;
        public int notificationsCount = 0;

        // Propriétés pour le formulaire de plainte
        public bool ShowPlainteForm { get; private set; }
        public string PlainteContenu { get; set; } = "";
        public string PlainteError { get; set; } = "";
        public string PlainteSuccess { get; set; } = "";
        public bool IsSubmittingPlainte { get; private set; }

        private int? _currentUserId;
        private int? _signalementId;
        
        public bool IsEmailNotVerified { get; private set; }
        public bool ShowEmailVerificationModal { get; private set; }
        public bool IsResendingEmail { get; private set; }
        public string EmailVerificationMessage { get; private set; } = "";

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public MainLayoutViewModel(
            ICompteService compteService,
            IImageService imageService,
            IPlainteService plainteService,
            ISignalementService signalementService,
            IServiceConnexion connexionService,
            IAnnonceService annonceService,
            ISignalRService signalRService,
            INotificationService notificationService,
            NotificationService notificationToastService,
            FavoriStateService favorisStateService)
        {
            _compteService = compteService;
            _imageService = imageService;
            _plainteService = plainteService;
            _signalementService = signalementService;
            _connexionService = connexionService;
            _annonceService = annonceService;
            _signalRService = signalRService;
            _notificationService = notificationService;
            _notificationToastService = notificationToastService;
            _favorisStateService = favorisStateService;
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
                    _currentUserId = compte.IdCompte;
                    IsAdmin = compte.TypeCompte == "Administrateur";
                    IsAccountSuspended = compte.EstSuspendu;
                    IsEmailNotVerified = !compte.EmailVerif; // ✅ Nouveau

                    if (!IsAccountSuspended && !IsEmailNotVerified)
                    {
                        await LoadProfileImage(compte.IdCompte);
                        // ... reste du code
                    }
                    else if (IsAccountSuspended)
                    {
                        await LoadActiveSignalement();
                    }
                }
            }
            catch
            {
                IsConnected = false;
            }

            _refreshUI?.Invoke();
        }

        private async Task LoadActiveSignalement()
        {
            try
            {
                var signalements = await _signalementService.GetAllSignalementsAsync();

                var signalement = signalements.FirstOrDefault(s =>
                    s.IdCompteSignale == _currentUserId &&
                    s.IdEtatSignalement == 1);

                if (signalement != null)
                {
                    _signalementId = signalement.IdSignalement;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du signalement: {ex.Message}");
            }
        }

        // ✅ Callback appelé quand les favoris changent
        private void HandleFavorisChanged()
        {
            Console.WriteLine($"📊 Favoris mis à jour: {_favorisStateService.GetFavorisIds().Count} annonces");
            _refreshUI?.Invoke();
        }

        private void HandlePriceDropNotification(PriceDropNotification notif)
        {
            Console.WriteLine($"🔔 Baisse de prix détectée: {notif.AnnonceLibelle} ({notif.OldPrice}€ → {notif.NewPrice}€)");
            notificationsCount++;
            _refreshUI?.Invoke();
        }

        public async Task RefreshNotificationCount()
        {
            if (_currentUserId.HasValue)
            {
                notificationsCount = await _notificationService.GetUnreadCountAsync(_currentUserId.Value);
                _refreshUI?.Invoke();
            }
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
            _nav?.NavigateTo("/notifications");
            _refreshUI?.Invoke();
        }

        // ========== GESTION DES PLAINTES ==========

        public void OpenPlainteForm()
        {
            ShowPlainteForm = true;
            PlainteContenu = "";
            PlainteError = "";
            PlainteSuccess = "";
            _refreshUI?.Invoke();
        }

        public void CancelPlainte()
        {
            ShowPlainteForm = false;
            PlainteContenu = "";
            PlainteError = "";
            PlainteSuccess = "";
            _refreshUI?.Invoke();
        }

        public async Task SubmitPlainte()
        {
            PlainteError = "";
            PlainteSuccess = "";

            // Validation
            if (string.IsNullOrWhiteSpace(PlainteContenu))
            {
                PlainteError = "Veuillez décrire le motif de votre plainte";
                _refreshUI?.Invoke();
                return;
            }

            if (PlainteContenu.Length < 10)
            {
                PlainteError = "La plainte doit contenir au moins 10 caractères";
                _refreshUI?.Invoke();
                return;
            }

            if (PlainteContenu.Length > 1000)
            {
                PlainteError = "La plainte ne peut pas dépasser 1000 caractères";
                _refreshUI?.Invoke();
                return;
            }

            if (!_currentUserId.HasValue)
            {
                PlainteError = "Erreur d'identification utilisateur";
                _refreshUI?.Invoke();
                return;
            }

            IsSubmittingPlainte = true;
            _refreshUI?.Invoke();

            try
            {
                if (!_signalementId.HasValue)
                {
                    var signalementDto = new SignalementCreateDTO
                    {
                        IdCompteSignalant = _currentUserId.Value,
                        IdCompteSignale = _currentUserId.Value,
                        IdTypeSignalement = 10,
                        DescriptionSignalement = "Contestation de suspension de compte"
                    };

                    var createdSignalement = await _signalementService.CreateAsync(signalementDto);

                    if (createdSignalement != null)
                    {
                        var signalements = await _signalementService.GetAllSignalementsAsync();
                        var newSignalement = signalements
                            .OrderByDescending(s => s.DateCreationSignalement)
                            .FirstOrDefault(s => s.IdCompteSignalant == _currentUserId.Value);

                        if (newSignalement != null)
                        {
                            _signalementId = newSignalement.IdSignalement;
                        }
                    }
                }

                if (!_signalementId.HasValue)
                {
                    PlainteError = "Impossible de créer le signalement";
                    return;
                }

                var plainteDto = new PlainteCreateDTO
                {
                    IdCompte = _currentUserId.Value,
                    IdSignalement = _signalementId.Value,
                    Description = PlainteContenu,
                    IdEtat = 1
                };

                var result = await _plainteService.PostWithErrorHandlingAsync(plainteDto);

                if (result.Success && result.Data != null)
                {
                    _notificationToastService.ShowSuccess(
                        "Plainte envoyée",
                        "Votre plainte a été transmise avec succès. Nos équipes l'examineront dans les plus brefs délais."
                    );

                    PlainteContenu = "";
                    PlainteSuccess = "Votre plainte a été envoyée avec succès.";
                    ShowPlainteForm = false;
                }
                else
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        PlainteError = result.ErrorMessage;
                    }
                    else
                    {
                        PlainteError = "Une erreur est survenue lors de l'envoi de votre plainte";
                    }

                    _notificationToastService.ShowError(
                        "Plainte non envoyée",
                        PlainteError
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'envoi de la plainte: {ex.Message}");
                PlainteError = "Une erreur est survenue lors de l'envoi de votre plainte";

                _notificationToastService.ShowError(
                    "Erreur",
                    PlainteError
                );
            }
            finally
            {
                IsSubmittingPlainte = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task LogoutSuspendedAccount()
        {
            try
            {
                await _connexionService.LogOutUser();
                await Task.Delay(100);
                _nav?.NavigateTo("/connexion", forceLoad: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur déconnexion: {ex.Message}");
                _nav?.NavigateTo("/connexion", forceLoad: true);
            }
        }
        
        private void HandleOffreNotification(OffreNotification notif)
        {
            notificationsCount++;
            
            _notificationToastService.ShowInfo(
                "💰 Nouvelle offre reçue !",
                $"Une offre de {notif.Valeur}€ a été faite sur votre annonce : {notif.AnnonceLibelle}",
                "/notifications"
            );
    
            _refreshUI?.Invoke();
        }
        
        public async Task RenvoyerEmailVerification()
        {
            if (!_currentUserId.HasValue) return;

            IsResendingEmail = true;
            EmailVerificationMessage = "";
            _refreshUI?.Invoke();

            try
            {
                var response = await _compteService.RenvoyerEmailVerification(_currentUserId.Value);

                if (response.IsSuccessStatusCode)
                {
                    EmailVerificationMessage = "✅ Email de vérification renvoyé avec succès ! Vérifiez votre boîte de réception.";
                    _notificationToastService.ShowSuccess(
                        "Email envoyé",
                        "Un nouvel email de vérification a été envoyé"
                    );
                }
                else
                {
                    EmailVerificationMessage = "❌ Erreur lors de l'envoi de l'email. Veuillez réessayer.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur renvoiEmail: {ex.Message}");
                EmailVerificationMessage = "❌ Erreur lors de l'envoi de l'email.";
            }
            finally
            {
                IsResendingEmail = false;
                _refreshUI?.Invoke();
            }
        }

        public void OpenEmailVerificationModal()
        {
            ShowEmailVerificationModal = true;
            EmailVerificationMessage = "";
            _refreshUI?.Invoke();
        }

        public void CloseEmailVerificationModal()
        {
            ShowEmailVerificationModal = false;
            EmailVerificationMessage = "";
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
                _signalRService.OnOffreReceived -= HandleOffreNotification;
            }

            if (_favorisStateService != null)
            {
                _favorisStateService.OnFavorisChanged -= HandleFavorisChanged;
            }
        }
    }
}
