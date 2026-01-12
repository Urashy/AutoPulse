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
        private readonly IAvisService _avisService;
        private readonly IMoyenPaiementService _moyenPaiementService;
        private readonly ICarteBancaireService _carteBancaireService;

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

        // Gestion de la Modale de Paiement (CB)
        public bool ShowPaiementModal { get; set; } = false;
        public int IdCbUse { get; set; } = 0;

        // États du paiement pour les autres méthodes
        public bool IsProcessingPayment { get; private set; } = false;
        public string PaymentError { get; private set; } = "";

        // Limite légale pour le paiement en espèces (ex: 1000€)
        public bool CanPayCash => PrixFinal <= 1000;

        // Gestion des Cartes et Moyens de Paiement
        public List<MoyenPaiementDTO> MoyensPaiement { get; private set; } = new();
        public List<CarteBancaireDTO> MesCartes { get; private set; } = new();

        // Données Avis
        public int NoteAvis { get; set; } = 5;
        public string ContenuAvis { get; set; } = string.Empty;
        public bool AvisEnvoye { get; private set; } = false;
        public bool IsSendingAvis { get; private set; } = false;

        public bool IsDownloadingFacture { get; set; } = false;
        public string? FactureErrorMessage { get; set; }

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public CommandeDetailViewModel(
            ICommandeService commandeService,
            ICompteService compteService,
            IAnnonceService annonceService,
            IImageService imageService,
            NotificationService notificationService,
            IConversationService conversationService,
            IAvisService avisService,
            IMoyenPaiementService moyenPaiementService,
            ICarteBancaireService carteBancaireService)
        {
            _commandeService = commandeService;
            _compteService = compteService;
            _annonceService = annonceService;
            _imageService = imageService;
            _notificationService = notificationService;
            _conversationService = conversationService;
            _avisService = avisService;
            _moyenPaiementService = moyenPaiementService;
            _carteBancaireService = carteBancaireService;
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

                await LoadPaymentData();
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

        private async Task LoadPaymentData()
        {
            try
            {
                MoyensPaiement = (await _moyenPaiementService.GetAllAsync()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement données paiement: {ex.Message}");
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
                4 => "Livraison émise",
                5 => "Livrée",
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
                4 => "livraison-emise",
                5 => "livree",
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
        // ACTIONS ACHETEUR - Paiement par Carte (Via Modal)
        // ============================================================================

        public void OpenPaiementModal()
        {
            ShowPaiementModal = true;
            _refreshUI?.Invoke();
        }

        public void SetIdCb(int id)
        {
            IdCbUse = id;
        }

        public async Task OnPaymentSuccess()
        {
            ShowPaiementModal = false;

            if (Commande == null) return;

            IsProcessingPayment = true;
            _refreshUI?.Invoke();

            try
            {
                // ID 1 pour Carte Bancaire (selon convention du projet)
                int idCb = 1;

                // Mise à jour : État 3 (Validé directement)
                Commande.IdEtatCommande = 3;
                Commande.MoyenPaiement = "Carte bancaire";

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande,
                    IdMoyenPaiement = idCb
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);

                _notificationService.ShowSuccess(
                    "Paiement validé",
                    "Votre paiement par carte a été accepté."
                );
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Erreur", "Erreur lors de la validation du paiement.");
                Console.WriteLine($"Erreur paiement CB: {ex.Message}");
            }
            finally
            {
                IsProcessingPayment = false;
                _refreshUI?.Invoke();
            }
        }

        // ============================================================================
        // ACTIONS ACHETEUR - Autres Paiements (Virement, Espèces, Chèque)
        // ============================================================================

        public async Task SelectAutrePaiement(int idMoyenPaiement)
        {
            if (Commande == null) return;

            // Vérification Espèces (ID 3)
            if (idMoyenPaiement == 3 && !CanPayCash)
            {
                _notificationService.ShowError("Non autorisé", "Le paiement en espèces est limité à 1000€.");
                return;
            }

            IsProcessingPayment = true;
            _refreshUI?.Invoke();

            try
            {
                // Mise à jour : État 2 (Paiement émis / En attente validation vendeur)
                Commande.IdEtatCommande = 2;

                // Mise à jour visuelle du libellé (optionnel, pour l'UI immédiate)
                Commande.MoyenPaiement = MoyensPaiement.FirstOrDefault(m => m.IdMoyenPaiement == idMoyenPaiement)?.TypePaiement ?? "Autre";

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande,
                    IdMoyenPaiement = idMoyenPaiement
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);

                _notificationService.ShowSuccess(
                    "Paiement déclaré",
                    "Le vendeur a été notifié de votre mode de paiement."
                );
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Erreur", "Impossible de mettre à jour le paiement.");
                Console.WriteLine($"Erreur autre paiement: {ex.Message}");
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
                Commande.IdEtatCommande = 3; // Paiement validé

                // On récupère l'ID moyen paiement actuel pour ne pas l'écraser par défaut
                int currentMoyenPaiementId = 1;
                var mp = MoyensPaiement.FirstOrDefault(m => m.TypePaiement == Commande.MoyenPaiement);
                if (mp != null) currentMoyenPaiementId = mp.IdMoyenPaiement;

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande,
                    IdMoyenPaiement = currentMoyenPaiementId
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
                Commande.IdEtatCommande = 4; // Livraison émise

                int currentMoyenPaiementId = 1;
                var mp = MoyensPaiement.FirstOrDefault(m => m.TypePaiement == Commande.MoyenPaiement);
                if (mp != null) currentMoyenPaiementId = mp.IdMoyenPaiement;

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande,
                    IdMoyenPaiement = currentMoyenPaiementId
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

                int currentMoyenPaiementId = 1;
                var mp = MoyensPaiement.FirstOrDefault(m => m.TypePaiement == Commande.MoyenPaiement);
                if (mp != null) currentMoyenPaiementId = mp.IdMoyenPaiement;

                CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                {
                    IdCommande = Commande.IdCommande,
                    IdVendeur = Commande.IdVendeur,
                    IdAcheteur = Commande.IdAcheteur,
                    IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                    IdOffre = Commande.Offre.IdOffre,
                    IdEtatCommande = Commande.IdEtatCommande,
                    IdMoyenPaiement = currentMoyenPaiementId
                };

                await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);

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

        // ============================================================================
        // ACTIONS DE FIN - Facture et Avis
        // ============================================================================

        public async Task GenererFacture()
        {
            if (Commande == null)
            {
                _notificationService.ShowError("Erreur", "Aucune commande sélectionnée");
                return;
            }

            try
            {
                IsDownloadingFacture = true;
                FactureErrorMessage = null;
                _refreshUI?.Invoke();

                Console.WriteLine($"[VM] Génération facture pour commande {Commande.IdCommande}");

                var success = await _commandeService.TelechargerFacturePdf(Commande.IdCommande);

                if (!success)
                {
                    FactureErrorMessage = "Impossible de télécharger la facture. Veuillez réessayer.";
                    _notificationService.ShowError("Erreur", "Impossible de télécharger la facture");
                    Console.WriteLine("[VM] Échec du téléchargement de la facture");
                }
                else
                {
                    _notificationService.ShowSuccess("Facture téléchargée", "La facture a été téléchargée avec succès");
                    Console.WriteLine("[VM] Facture téléchargée avec succès");
                }
            }
            catch (Exception ex)
            {
                FactureErrorMessage = $"Erreur lors du téléchargement : {ex.Message}";
                _notificationService.ShowError("Erreur", "Une erreur est survenue lors du téléchargement");
                Console.WriteLine($"[VM] Exception GenererFacture: {ex.Message}");
            }
            finally
            {
                IsDownloadingFacture = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task EnvoyerAvis()
        {
            if (Commande == null || !CurrentUserId.HasValue) return;

            if (string.IsNullOrWhiteSpace(ContenuAvis))
            {
                _notificationService.ShowError("Erreur", "Veuillez écrire un commentaire pour votre avis.");
                return;
            }

            IsSendingAvis = true;
            _refreshUI?.Invoke();

            try
            {
                var avisDTO = new AvisCreateDTO
                {
                    IdJugeur = CurrentUserId.Value,
                    IdJugee = IsAcheteur ? Commande.IdVendeur : Commande.IdAcheteur,
                    IdCommande = Commande.IdCommande,
                    ContenuAvis = ContenuAvis,
                    NoteAvis = NoteAvis
                };

                var result = await _avisService.CreateAvis(avisDTO);

                if (result.Success)
                {
                    AvisEnvoye = true;
                    _notificationService.ShowSuccess("Avis envoyé", "Merci pour votre retour !");
                }
                else
                {
                    _notificationService.ShowError("Erreur", result.ErrorMessage ?? "Impossible d'envoyer l'avis");
                }
            }
            catch (Exception ex)
            {
                _notificationService.ShowError("Erreur", "Une erreur est survenue lors de l'envoi de l'avis");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                IsSendingAvis = false;
                _refreshUI?.Invoke();
            }
        }
    }
}