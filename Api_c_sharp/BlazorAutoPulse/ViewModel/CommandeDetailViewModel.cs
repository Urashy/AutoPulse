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

        // États du paiement
        public bool ShowPaymentForm { get; private set; } = false;
        public string? SelectedPaymentType { get; private set; }
        public bool IsProcessingPayment { get; private set; } = false;
        public string PaymentError { get; private set; } = "";

        // Gestion des Cartes et Moyens de Paiement
        public List<MoyenPaiementDTO> MoyensPaiement { get; private set; } = new();
        public List<CarteBancaireDTO> MesCartes { get; private set; } = new();
        public int? SelectedCardId { get; set; }
        public bool IsCardModalVisible { get; set; } = false;

        // Données Avis
        public int NoteAvis { get; set; } = 5;
        public string ContenuAvis { get; set; } = string.Empty;
        public bool AvisEnvoye { get; private set; } = false;
        public bool IsSendingAvis { get; private set; } = false;

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

                // Si c'est l'acheteur et qu'il doit payer, on charge les infos de paiement
                if (IsAcheteur && Commande.IdEtatCommande == 1)
                {
                    await LoadPaymentData();
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

        private async Task LoadPaymentData()
        {
            try
            {
                MoyensPaiement = (await _moyenPaiementService.GetAllAsync()).ToList();
                if (CurrentUserId.HasValue)
                {
                    MesCartes = (await _carteBancaireService.GetCarteBancaireByCompte(CurrentUserId.Value)).ToList();
                    if (MesCartes.Any())
                    {
                        SelectedCardId = MesCartes.First().IdCarteBancaire;
                    }
                }
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
        // ACTIONS ACHETEUR - Paiement
        // ============================================================================

        // Gestion de la modale d'ajout de carte
        public void OpenAddCardModal()
        {
            IsCardModalVisible = true;
            _refreshUI?.Invoke();
        }

        public async Task OnCardAdded()
        {
            if (CurrentUserId.HasValue)
            {
                MesCartes = (await _carteBancaireService.GetCarteBancaireByCompte(CurrentUserId.Value)).ToList();
                // Sélectionner la dernière carte (supposée être celle ajoutée, ou par ID max)
                if (MesCartes.Any())
                {
                    SelectedCardId = MesCartes.MaxBy(c => c.IdCarteBancaire)?.IdCarteBancaire;
                }
            }
            _refreshUI?.Invoke();
        }

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
            PaymentError = "";
            _refreshUI?.Invoke();
        }

        public async Task ValidateCardPayment()
        {
            PaymentError = "";

            if (!SelectedCardId.HasValue)
            {
                PaymentError = "Veuillez sélectionner une carte bancaire ou en ajouter une nouvelle.";
                _refreshUI?.Invoke();
                return;
            }

            IsProcessingPayment = true;
            _refreshUI?.Invoke();

            try
            {
                // Simulation délai bancaire
                await Task.Delay(2000);

                // Récupération de l'ID du moyen de paiement "Carte bancaire"
                // On cherche celui qui contient "carte" ou on prend 1 par défaut
                int idMoyenPaiementCB = MoyensPaiement
                    .FirstOrDefault(m => m.TypePaiement.ToLower().Contains("carte"))?.IdMoyenPaiement ?? 1;

                if (Commande != null)
                {
                    // Paiement par carte validé directement -> État 3
                    Commande.IdEtatCommande = 3;
                    // Mise à jour locale pour l'affichage immédiat
                    Commande.MoyenPaiement = "Carte bancaire";

                    CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                    {
                        IdCommande = Commande.IdCommande,
                        IdVendeur = Commande.IdVendeur,
                        IdAcheteur = Commande.IdAcheteur,
                        IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                        IdOffre = Commande.Offre.IdOffre,
                        IdEtatCommande = Commande.IdEtatCommande,
                        // On enregistre le vrai moyen de paiement
                        IdMoyenPaiement = idMoyenPaiementCB
                    };

                    await _commandeService.UpdateCommandeAsync(Commande.IdCommande, Commandeup);
                }

                _notificationService.ShowSuccess(
                    "Paiement effectué",
                    "Votre paiement a été enregistré avec succès"
                );

                ShowPaymentForm = false;
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
                // Récupération de l'ID d'un moyen de paiement "Autre"
                // On prend le premier qui n'est PAS "Carte", ou 2 par défaut
                int idMoyenPaiementAutre = MoyensPaiement
                    .FirstOrDefault(m => !m.TypePaiement.ToLower().Contains("carte"))?.IdMoyenPaiement ?? 2;

                if (Commande != null)
                {
                    Commande.IdEtatCommande = 2; // Paiement émis
                    Commande.MoyenPaiement = "Autre";

                    CommandeUpdateDTO Commandeup = new CommandeUpdateDTO
                    {
                        IdCommande = Commande.IdCommande,
                        IdVendeur = Commande.IdVendeur,
                        IdAcheteur = Commande.IdAcheteur,
                        IdAnnonce = Commande.Offre != null ? Commande.Offre.IdAnnonce : Commande.Annonce!.IdAnnonce,
                        IdOffre = Commande.Offre.IdOffre,
                        IdEtatCommande = Commande.IdEtatCommande,
                        IdMoyenPaiement = idMoyenPaiementAutre
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
                Commande.IdEtatCommande = 3; // Paiement validé

                // On garde le moyen de paiement existant s'il est déjà défini
                int currentMoyenPaiementId = 1; // Valeur par défaut si inconnue
                if (!string.IsNullOrEmpty(Commande.MoyenPaiement))
                {
                    // Tentative de retrouver l'ID à partir du libellé actuel, sinon garde par défaut
                    var mp = MoyensPaiement.FirstOrDefault(m => m.TypePaiement == Commande.MoyenPaiement);
                    if (mp != null) currentMoyenPaiementId = mp.IdMoyenPaiement;
                }

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

                // On garde le moyen de paiement existant
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

                // On garde le moyen de paiement existant
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

        public void GenererFacture()
        {
            _notificationService.ShowInfo("Facture", "La fonctionnalité de téléchargement de facture sera bientôt disponible.");
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