using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class CommandeDetailViewModel
    {
        private readonly ICommandeService _commandeService;
        private readonly ICompteService _compteService;
        private readonly IAnnonceService _annonceService;
        private readonly IImageService _imageService;
        private readonly NotificationService _notificationService;
        private readonly IConversationService _conversationService;

        public CommandeDetailDTO? Commande { get; private set; }
        public AnnonceDTO? Annonce { get; private set; }
        public bool IsLoading { get; private set; } = true;
        public int? CurrentUserId { get; private set; }

        public bool IsAcheteur => CurrentUserId.HasValue &&
                                   Commande != null &&
                                   Commande.IdAcheteur == CurrentUserId.Value;

        public bool IsVendeur => CurrentUserId.HasValue &&
                                 Commande != null &&
                                 Commande.IdVendeur == CurrentUserId.Value;

        // États du paiement
        public bool ShowPaymentForm { get; private set; } = false;
        public string? SelectedPaymentType { get; private set; }
        public bool IsProcessingPayment { get; private set; } = false;
        public string PaymentError { get; private set; } = "";

        // Données carte bancaire
        public string CardNumber { get; set; } = "";
        public string CardExpiry { get; set; } = "";
        public string CardCvv { get; set; } = "";

        public decimal PrixFinal { get; private set; }

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public CommandeDetailViewModel(
            ICommandeService commandeService,
            ICompteService compteService,
            IAnnonceService annonceService,
            IImageService imageService,
            NotificationService notificationService,
            IConversationService conversationService)
        {
            _commandeService = commandeService;
            _compteService = compteService;
            _annonceService = annonceService;
            _imageService = imageService;
            _notificationService = notificationService;
            _conversationService = conversationService;
        }

        public async Task InitializeAsync(int idCommande, Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                // Récupérer l'utilisateur connecté
                try
                {
                    var user = await _compteService.GetMe();
                    CurrentUserId = user.IdCompte;
                }
                catch
                {
                    CurrentUserId = null;
                    _nav?.NavigateTo("/connexion");
                    return;
                }

                // Charger la commande
                Commande = await _commandeService.GetCommandeDetailById(idCommande);

                if (Commande == null)
                {
                    return;
                }

                // Vérifier que l'utilisateur est bien partie prenante
                if (!IsAcheteur && !IsVendeur)
                {
                    _notificationService.ShowError(
                        "Accès refusé",
                        "Vous n'avez pas accès à cette commande"
                    );
                    _nav?.NavigateTo("/compte");
                    return;
                }

                // Charger l'annonce si disponible
                if (Commande.Annonce != null)
                {
                    try
                    {
                        Annonce = Commande.Annonce;

                        // Récupérer le prix depuis l'offre acceptée si disponible
                        // Sinon utiliser le prix de l'annonce
                        PrixFinal = Annonce.Prix ?? 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur chargement annonce: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur initialisation commande: {ex.Message}");
                Commande = null;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public string GetStatutLabel()
        {
            if (Commande == null) return "";

            return Commande.IdEtatCommande switch
            {
                1 => "En attente de paiement",
                2 => "Paiement émis",
                3 => "Paiement validé",
                4 => "En attente de livraison",
                5 => "Livraison émise",
                6 => "Livrée",
                _ => "Statut inconnu"
            };
        }

        public string GetStatutClass()
        {
            if (Commande == null) return "";

            return Commande.IdEtatCommande switch
            {
                1 => "en-attente",
                2 => "payee",
                3 => "payement-validee",
                4 => "en-attente-livraison",
                5 => "livraison",
                6 => "livree",
                _ => ""
            };
        }

        public string GetAnnonceImage()
        {
            if (Annonce == null) return "https://via.placeholder.com/200x150?text=Pas+d'image";

            try
            {
                return _imageService.GetFirstImage(Annonce.IdVoiture);
            }
            catch
            {
                return "https://via.placeholder.com/200x150?text=Pas+d'image";
            }
        }

        // ============================================================================
        // ACTIONS ACHETEUR - Paiement
        // ============================================================================

        public void SelectPaymentCarte()
        {
            SelectedPaymentType = "carte";
            ShowPaymentForm = true;
            PaymentError = "";
            _refreshUI?.Invoke();
        }

        public void SelectPaymentAutre()
        {
            SelectedPaymentType = "autre";
            ShowPaymentForm = true;
            PaymentError = "";
            _refreshUI?.Invoke();
        }

        public void CancelPayment()
        {
            ShowPaymentForm = false;
            SelectedPaymentType = null;
            CardNumber = "";
            CardExpiry = "";
            CardCvv = "";
            PaymentError = "";
            _refreshUI?.Invoke();
        }

        public async Task ValidateCardPayment()
        {
            PaymentError = "";

            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < 16)
            {
                PaymentError = "Numéro de carte invalide";
                _refreshUI?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(CardExpiry) || CardExpiry.Length != 5)
            {
                PaymentError = "Date d'expiration invalide (format: MM/AA)";
                _refreshUI?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(CardCvv) || CardCvv.Length != 3)
            {
                PaymentError = "CVV invalide";
                _refreshUI?.Invoke();
                return;
            }

            IsProcessingPayment = true;
            _refreshUI?.Invoke();

            try
            {
                await Task.Delay(2000);

                // TODO: Appel API pour enregistrer le paiement
                // await _commandeService.ValidatePayment(Commande.IdCommande, cardData);

                // Mise à jour de l'état de la commande
                if (Commande != null)
                {
                    Commande.IdEtatCommande = 2; 
                }

                _notificationService.ShowSuccess(
                    "Paiement effectué",
                    "Votre paiement a été enregistré avec succès"
                );

                // Reset du formulaire
                ShowPaymentForm = false;
                CardNumber = "";
                CardExpiry = "";
                CardCvv = "";
            }
            catch (Exception ex)
            {
                PaymentError = "Erreur lors du traitement du paiement";
                Console.WriteLine($"Erreur paiement: {ex.Message}");
            }
            finally
            {
                IsProcessingPayment = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task ConfirmAutrePayment()
        {
            IsProcessingPayment = true;
            _refreshUI?.Invoke();

            try
            {
                // TODO: Appel API pour confirmer le paiement autre moyen
                // await _commandeService.ConfirmOtherPayment(Commande.IdCommande);

                if (Commande != null)
                {
                    Commande.IdEtatCommande = 2; // Paiement en cours
                }

                _notificationService.ShowSuccess(
                    "Confirmation envoyée",
                    "Le vendeur sera notifié de votre paiement"
                );

                ShowPaymentForm = false;
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de confirmer le paiement"
                );
                Console.WriteLine($"Erreur confirmation paiement: {ex.Message}");
            }
            finally
            {
                IsProcessingPayment = false;
                _refreshUI?.Invoke();
            }
        }

        // ============================================================================
        // ACTIONS VENDEUR - Confirmation réception
        // ============================================================================

        public async Task ConfirmPaymentReceived()
        {
            if (Commande == null) return;

            try
            {
                // TODO: Appel API pour confirmer la réception du paiement
                // await _commandeService.ConfirmPaymentReceived(Commande.IdCommande);

                Commande.IdEtatCommande = 3; // Paiement validé

                _notificationService.ShowSuccess(
                    "Paiement confirmé",
                    "La transaction a été validée avec succès"
                );

                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de confirmer la réception du paiement"
                );
                Console.WriteLine($"Erreur confirmation réception: {ex.Message}");
            }
        }

        // ============================================================================
        // ACTIONS COMMUNES - Contact
        // ============================================================================

        public async Task ContacterVendeur()
        {
            if (Commande == null) return;

            // Créer ou ouvrir une conversation avec le vendeur
            try
            {
                var conversations = await _conversationService.GetConversationsByCompteID(CurrentUserId ?? 0);

                // Chercher une conversation existante avec ce vendeur pour cette annonce
                var existingConv = conversations.FirstOrDefault(c =>
                    c.IdAnnonce == Commande.Annonce?.IdAnnonce &&
                    c.ParticipantPseudo == Commande.PseudoVendeur
                );

                if (existingConv != null)
                {
                    _nav?.NavigateTo("/conversations");
                }
                else
                {
                    // Créer une nouvelle conversation
                    _nav?.NavigateTo($"/annonce/{Commande.Annonce?.IdAnnonce}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur contact vendeur: {ex.Message}");
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de contacter le vendeur"
                );
            }
        }

        public async Task ContacterAcheteur()
        {
            if (Commande == null) return;

            try
            {
                var conversations = await _conversationService.GetConversationsByCompteID(CurrentUserId ?? 0);

                var existingConv = conversations.FirstOrDefault(c =>
                    c.IdAnnonce == Commande.Annonce?.IdAnnonce &&
                    c.ParticipantPseudo == Commande.PseudoAcheteur
                );

                if (existingConv != null)
                {
                    _nav?.NavigateTo("/conversations");
                }
                else
                {
                    _nav?.NavigateTo($"/annonce/{Commande.Annonce?.IdAnnonce}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur contact acheteur: {ex.Message}");
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de contacter l'acheteur"
                );
            }
        }
    }
}