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
        public bool IsLoading { get; private set; } = true;
        public int? CurrentUserId { get; private set; }

        public bool IsAcheteur => CurrentUserId.HasValue &&
                                  Commande != null &&
                                  Commande.IdAcheteur == CurrentUserId.Value;

        public bool IsVendeur => CurrentUserId.HasValue &&
                                 Commande != null &&
                                 Commande.IdVendeur == CurrentUserId.Value;

        public decimal PrixFinal { get; private set; } = 0m;

        // États du paiement
        public bool ShowPaymentForm { get; private set; } = false;
        public string? SelectedPaymentType { get; private set; }
        public bool IsProcessingPayment { get; private set; } = false;
        public string PaymentError { get; private set; } = "";

        // Données carte bancaire
        public string CardNumber { get; set; } = "";
        public string CardExpiry { get; set; } = "";
        public string CardCvv { get; set; } = "";

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
            PrixFinal = 0m;
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
                    IsLoading = false;
                    _refreshUI?.Invoke();
                    return;
                }

                // Vérifier que l'utilisateur est bien partie prenante
                if (!IsAcheteur && !IsVendeur)
                {
                    _notificationService.ShowError(
                        "Accès refusé",
                        "Vous n'avez pas accès à cette commande"
                    );
                    IsLoading = false;
                    _refreshUI?.Invoke();
                    _nav?.NavigateTo("/compte");
                    return;
                }

                // Définir le prix final
                if (Commande.Offre != null && Commande.Offre.Valeur > 0)
                {
                    PrixFinal = Commande.Offre.Valeur;
                }
                else if (Commande.Annonce?.Prix != null && Commande.Annonce.Prix > 0)
                {
                    PrixFinal = Commande.Annonce.Prix.Value;
                }
                else
                {
                    PrixFinal = 0m;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur initialisation commande: {ex.Message}");
                Commande = null;
                PrixFinal = 0m;
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
                3 => "Paiement validé / En prépa",
                4 => "Livraison émise", // Anciennement 5
                5 => "Livrée", // Anciennement 6
                _ => "Statut inconnu"
            };
        }

        public string GetStatutClass()
        {
            if (Commande == null) return "";

            return Commande.IdEtatCommande switch
            {
                1 => "en-attente",
                2 => "paiement-emis",
                3 => "paiement-valide",
                4 => "livraison-emise", // Anciennement 5
                5 => "livree", // Anciennement 6
                _ => ""
            };
        }

        public string GetAnnonceImage()
        {
            if (Commande?.Annonce == null) return "https://via.placeholder.com/200x150?text=Pas+d'image";

            try
            {
                return _imageService.GetFirstImage(Commande.Annonce.IdVoiture);
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

                // TODO: Appel API pour enregistrer le paiement par carte
                // await _commandeService.ValidateCardPayment(Commande.IdCommande, new CardPaymentDTO { ... });


                // Mise à jour de l'état de la commande
                if (Commande != null)
                {
                    // Paiement direct par carte = Validé directement (état 3)
                    Commande.IdEtatCommande = 3;

                    CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                    {
                        IdCommande = Commande.IdCommande,
                        IdVendeur = Commande.IdVendeur,
                        IdAcheteur = Commande.IdAcheteur,
                        IdAnnonce = Commande.Offre.IdAnnonce,
                        IdMoyenPaiement = 1,
                        IdOffre = Commande.Offre.IdOffre,
                        IdEtatCommande = Commande.IdEtatCommande
                    };

                    await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);
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
                // await _commandeService.EmitPayment(Commande.IdCommande);


                

                if (Commande != null)
                {
                    Commande.IdEtatCommande = 2; // Paiement émis

                    CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                    {
                        IdCommande = Commande.IdCommande,
                        IdVendeur = Commande.IdVendeur,
                        IdAcheteur = Commande.IdAcheteur,
                        IdAnnonce = Commande.Offre.IdAnnonce,
                        IdMoyenPaiement = 1,
                        IdOffre = Commande.Offre.IdOffre,
                        IdEtatCommande = Commande.IdEtatCommande
                    };

                    await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);
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
        // ACTIONS VENDEUR - Confirmation réception paiement
        // ============================================================================

        public async Task ConfirmPaymentReceived()
        {
            if (Commande == null) return;

            try
            {
                // TODO: Appel API pour confirmer la réception du paiement
                // await _commandeService.ConfirmPaymentReceived(Commande.IdCommande);

                Commande.IdEtatCommande = 3; // Paiement validé

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre.IdAnnonce,
                    IdMoyenPaiement = 1,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);

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
        // ACTIONS VENDEUR - Émission de la livraison
        // ============================================================================

        public async Task EmitDelivery()
        {
            if (Commande == null) return;

            try
            {
                // TODO: Appel API pour émettre la livraison
                // await _commandeService.EmitDelivery(Commande.IdCommande);

                Commande.IdEtatCommande = 4; // Livraison émise (Passage de 5 à 4)

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre.IdAnnonce,
                    IdMoyenPaiement = 1,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);

                _notificationService.ShowSuccess(
                    "Livraison émise",
                    "L'acheteur a été notifié que le véhicule est prêt"
                );

                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible d'émettre la livraison"
                );
                Console.WriteLine($"Erreur émission livraison: {ex.Message}");
            }
        }

        // ============================================================================
        // ACTIONS ACHETEUR - Confirmation réception véhicule
        // ============================================================================

        public async Task ConfirmVehicleReceived()
        {
            if (Commande == null) return;

            try
            {

                Commande.IdEtatCommande = 5;

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre.IdAnnonce,
                    IdMoyenPaiement = 1,
                    IdOffre = Commande.Offre.IdOffre,

                    IdEtatCommande = Commande.IdEtatCommande
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande,Commandeup);

                _notificationService.ShowSuccess(
                    "Commande terminée",
                    "Félicitations pour votre achat !"
                );

                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de confirmer la réception du véhicule"
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

            try
            {
                var conversations = await _conversationService.GetConversationsByCompteID(CurrentUserId ?? 0);

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