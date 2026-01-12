using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Helper;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using BlazorAutoPulse.Service;
using Microsoft.AspNetCore.Components;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceDetailViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly IFavorisService _favorisService;
        private readonly ICompteService _compteService;
        private readonly ICouleurService _couleurService;
        private readonly NotificationService _notificationService;
        private readonly IConversationService _conversationService;
        private readonly IService<VueDTO> _vueService;
        private readonly IOffreService _offreService;
        private readonly IMessageService _messageService;
        private readonly FavoriStateService _favorisStateService;
        private readonly ConversationStateService _conversationStateService;
        private readonly IAPourConversationService _iaPourConversationService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public IEnumerable<AnnonceDTO> AnnonceSimilaires { get; private set; } 
        public List<int> ImageIds { get; private set; } = new();
        public int CurrentImageIndex { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;
        public bool IsFavorite { get; private set; } = false;
        public int? CurrentUserId { get; private set; }
        public bool IsLoadingImages { get; private set; } = false;

        // Propriétés pour le visualiseur 3D
        public bool show3DViewer { get; private set; } = false;
        public bool is3DReady { get; private set; } = false;
        public bool isLoading3D { get; private set; } = false;

        public List<CouleurDTO> couleurDisponible { get; set; }
        public string selectedColor { get; private set; }

        // Propriétés pour le menu d'options
        public bool IsOptionsMenuOpen { get; private set; } = false;
        public string? infoOptionAnnonce { get; private set; } = null;

        public bool estMasquer = false;

        public string ProfileImageSource { get; private set; } = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
        
        public string modifAnnonceUrl { get; set; }
        
        // IA
        private readonly IIAService _iaService;

        // États des popups IA
        public bool showPriceWarningPopup { get; set; } = false;
        public bool showPriceLoadingPopup { get; set; } = false;
        public bool showAdjustmentWarningPopup { get; set; } = false;
        public bool showAdjustmentLoadingPopup { get; set; } = false;
        public bool showAdjustmentResultPopup { get; set; } = false;
        
        // Propriétés pour la popup de contact
        public bool showContactPopup { get; set; } = false;
        public string contactMessage { get; set; } = "";
        public bool isLoadingContact { get; set; } = false;
        public string contactError { get; set; } = "";

        // Résultats IA
        public ResultatPrediction? priceResult { get; set; }
        public ResultatAjustement? adjustmentResult { get; set; }
        public List<string> missingFields { get; set; } = new();

        public int CurrentSimilarIndex { get; private set; } = 0;
        private const int AdsPerPage = 5; // 5 cartes visibles à la fois

        // Nombre total de pages (positions) possibles
        public int TotalSimilarPages =>
            Math.Max(0, (AnnonceSimilaires?.Count() ?? 0) - AdsPerPage + 1);

        // Vérifications pour la navigation
        public bool CanGoPreviousSimilar => CurrentSimilarIndex > 0;
        public bool CanGoNextSimilar => CurrentSimilarIndex < TotalSimilarPages - 1;

        // Propriété calculée pour le style de transformation CSS
        public int CurrentSimilarPage => CurrentSimilarIndex; // Pour les dots

        // OFFRES
        public bool showOffreMode { get; set; } = false;
        public decimal offreAmount { get; set; } = 0;
        public string offreError { get; set; } = "";
        
        // Conversation existante
        public bool ConvExistante { get; set; }
        
        // Propriétés pour les données de mise en avant
        public PaiementDTO paiement { get; set; }
        public int NombreTotalPaiements => 
            paiement.NbSemaineMiseEnAvantOr + 
            paiement.NbSemaineMiseEnAvantPlatine + 
            paiement.NbSemaineMiseEnAvantDiamant;

        public decimal PrixTotal =>
            (paiement.NbSemaineMiseEnAvantOr * paiement.PrixMiseEnAvantOr) +
            (paiement.NbSemaineMiseEnAvantPlatine * paiement.PrixMisedAvantPlatine) +
            (paiement.NbSemaineMiseEnAvantDiamant * paiement.PrixMiseEnAvantDiamant);

        private Action? _refreshUI;
        private IJSRuntime? _jsRuntime;
        private NavigationManager _nav;

        public AnnonceDetailViewModel(
            IAnnonceService annonceService,
            IPostImageService postImageService,
            IFavorisService favorisService,
            ICompteService compteService,
            IImageService imageService,
            ICouleurService couleurService,
            IService<VueDTO> vueService,
            IConversationService conversationService,
            IIAService  iaService,
            NotificationService notificationService,
            IOffreService offreService,
            IMessageService messageService,
            FavoriStateService favorisStateService,
            ConversationStateService conversationStateService,
            IAPourConversationService iaPourConversationService)
        {
            _annonceService = annonceService;
            _postImageService = postImageService;
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
            _couleurService = couleurService;
            _iaService = iaService;
            _notificationService = notificationService;
            _conversationService = conversationService;
            _vueService = vueService;
            _offreService = offreService;
            _messageService = messageService;
            _favorisStateService = favorisStateService;
            _conversationStateService = conversationStateService;
            _iaPourConversationService = iaPourConversationService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI, IJSRuntime jsRuntime, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _jsRuntime = jsRuntime;
            _nav = nav;
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                try
                {
                    CompteDetailDTO user = await _compteService.GetMe();
                    CurrentUserId = user.IdCompte;
                }
                catch
                {
                    CurrentUserId = null;
                }

                Annonce = await _annonceService.GetAnnonceDetailById(idAnnonce);
            
                if (Annonce != null)
                {
                    await LoadVendeurProfileImage(Annonce.IdVendeur);
                    await LoadAllImages();
                    await GetPaiements();
                    modifAnnonceUrl = $"/modifier-annonce/{Annonce.IdAnnonce}";
                }

                AnnonceSimilaires = await _annonceService.GetAnnoncesSimilaires(idAnnonce);
                

                couleurDisponible = await _couleurService.GetCouleursByVoitureId(Annonce.IdVoiture);
                selectedColor = couleurDisponible?.FirstOrDefault()?.CodeHexaCouleur;

                // Vérifier si l'annonce est en favoris
                if (Annonce != null && CurrentUserId.HasValue)
                {
                    IsFavorite = _favorisStateService.IsFavorite(idAnnonce);

                    await _vueService.CreateAsync(new VueDTO
                    {
                        IdAnnonce = idAnnonce,
                        IdCompte = CurrentUserId ?? 0
                    });
                }

                ExistConv();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'annonce: {ex.Message}");
                Annonce = null;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();

                // Charger le modèle 3D automatiquement après le chargement de l'annonce
                if (Annonce != null && !string.IsNullOrEmpty(Annonce.LienModeleBlender))
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(500);
                        await Initialize3DViewer();
                    });
                }
            }

            EstMasquerAnnonce();
        }

        // ✅ Nouvelle méthode pour charger tous les IDs d'images
        private async Task LoadAllImages()
        {
            if (Annonce == null) return;

            IsLoadingImages = true;
            _refreshUI?.Invoke();

            try
            {
                ImageIds = await _imageService.GetAllImageIdsByVoitureId(Annonce.IdVoiture);

                if (!ImageIds.Any())
                {
                    Console.WriteLine("⚠️ Aucune image trouvée pour cette voiture");
                    ImageIds = new List<int>();
                }
                else
                {
                    Console.WriteLine($"✅ {ImageIds.Count} image(s) chargée(s)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement des images: {ex.Message}");
                ImageIds = new List<int>();
            }
            finally
            {
                IsLoadingImages = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task ToggleFavorite()
        {
            if (!CurrentUserId.HasValue || Annonce == null)
            {
                Console.WriteLine("Utilisateur non connecté ou annonce invalide");
                return;
            }

            try
            {
                // ✅ Utilisation du FavorisStateService qui gère automatiquement SignalR
                bool newStatus = await _favorisStateService.ToggleFavorisAsync(Annonce.IdAnnonce);
                IsFavorite = newStatus;
        
                Console.WriteLine($"✅ Favori toggled pour annonce {Annonce.IdAnnonce}: {IsFavorite}");
        
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors de l'ajout/suppression du favori: {ex.Message}");
            }
        }

        // ✅ Méthodes améliorées pour les images
        public string GetCurrentImage()
        {
            if (CurrentImageIndex >= 0 && CurrentImageIndex < ImageIds.Count)
            {
                return _imageService.GetImage(ImageIds[CurrentImageIndex]);
            }
            return "https://via.placeholder.com/800x600?text=Aucune+image";
        }

        public string GetImageUrl(int imageId)
        {
            return _imageService.GetImage(imageId);
        }

        public void NextImage()
        {
            if (CurrentImageIndex < ImageIds.Count - 1)
            {
                CurrentImageIndex++;
                _refreshUI?.Invoke();
            }
        }

        public void PreviousImage()
        {
            if (CurrentImageIndex > 0)
            {
                CurrentImageIndex--;
                _refreshUI?.Invoke();
            }
        }

        public void SelectImage(int index)
        {
            if (index >= 0 && index < ImageIds.Count)
            {
                CurrentImageIndex = index;
                _refreshUI?.Invoke();
            }
        }

        public bool CanGoNext => CurrentImageIndex < ImageIds.Count - 1;
        public bool CanGoPrevious => CurrentImageIndex > 0;

        // Méthodes pour le visualiseur 3D
        public async Task ToggleViewer3D()
        {
            show3DViewer = !show3DViewer;
            _refreshUI?.Invoke();

            if (show3DViewer && !is3DReady)
            {
                isLoading3D = true;
                _refreshUI?.Invoke();

                await Task.Delay(300);
                await Initialize3DViewer();
            }
        }

        public async Task Initialize3DViewer()
        {
            if (_jsRuntime == null || Annonce == null)
            {
                Console.WriteLine("JSRuntime ou Annonce est null");
                return;
            }

            try
            {
                Console.WriteLine("Début de l'initialisation du visualiseur 3D");

                var containerExists = await _jsRuntime.InvokeAsync<bool>("eval",
                    "document.getElementById('car3DViewer') !== null");

                Console.WriteLine($"Le conteneur existe: {containerExists}");

                if (!containerExists)
                {
                    Console.WriteLine("Le conteneur n'existe pas encore, on réessaie dans 500ms...");
                    await Task.Delay(500);

                    containerExists = await _jsRuntime.InvokeAsync<bool>("eval",
                        "document.getElementById('car3DViewer') !== null");

                    if (!containerExists)
                    {
                        Console.WriteLine("Le conteneur n'existe toujours pas !");
                        isLoading3D = false;
                        _refreshUI?.Invoke();
                        return;
                    }
                }

                string modelUrl = Annonce.LienModeleBlender;

                Console.WriteLine($"Chargement du modèle: {modelUrl}");

                await _jsRuntime.InvokeVoidAsync("car3DViewer.init", "car3DViewer", modelUrl);

                var animations = await _jsRuntime.InvokeAsync<string[]>("car3DViewer.getAnimationNames");

                Console.WriteLine("Visualiseur 3D initialisé avec succès");
                isLoading3D = false;
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du visualiseur 3D: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                isLoading3D = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task ChangeCouleur(string hexColor)
        {
            selectedColor = hexColor;

            if (show3DViewer && _jsRuntime != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("car3DViewer.changeColor", hexColor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du changement de couleur: {ex.Message}");
                }
            }
        }

        public void ToggleOptionsMenu()
        {
            IsOptionsMenuOpen = !IsOptionsMenuOpen;
            if (!IsOptionsMenuOpen)
            {
                infoOptionAnnonce = null;
            }
            _refreshUI?.Invoke();
        }

        public void SetInfoOption(string? info)
        {
            infoOptionAnnonce = info;
            _refreshUI?.Invoke();
        }

        public async Task MasquerAnnonce()
        {
            AnnonceUpdateDTO annonceChange = new AnnonceUpdateDTO()
            {
                IdAnnonce = Annonce.IdAnnonce,
                Libelle = Annonce.Libelle,
                IdCompte = Annonce.IdVendeur,
                IdEtatAnnonce = 4,
                IdAdresse = Annonce.IdAdresse,
                IdVoiture = Annonce.IdVoiture,
                IdMiseEnAvant = Annonce.IdMiseEnAvant,
                DatePublication = Annonce.DatePublication,
                Prix = Annonce.Prix,
                Description = Annonce.Libelle
            };
            await _annonceService.UpdateAnnonceAsync(Annonce.IdAnnonce, annonceChange);
            IsOptionsMenuOpen = false;

            await EstMasquerAnnonce();
            _refreshUI?.Invoke();
        }

        public async Task DemasquerAnnonce()
        {
            AnnonceUpdateDTO annonceChange = new AnnonceUpdateDTO()
            {
                IdAnnonce = Annonce.IdAnnonce,
                Libelle = Annonce.Libelle,
                IdCompte = Annonce.IdVendeur,
                IdEtatAnnonce = 1,
                IdAdresse = Annonce.IdAdresse,
                IdVoiture = Annonce.IdVoiture,
                IdMiseEnAvant = Annonce.IdMiseEnAvant,
                DatePublication = Annonce.DatePublication,
                Prix = Annonce.Prix,
                Description = Annonce.Libelle
            };
            await _annonceService.UpdateAnnonceAsync(Annonce.IdAnnonce, annonceChange);
            IsOptionsMenuOpen = false;

            await EstMasquerAnnonce();
            _refreshUI?.Invoke();
        }

        public async Task EstMasquerAnnonce()
        {
            estMasquer = await _annonceService.EstMasquerAsync(Annonce.IdAnnonce);
            _refreshUI?.Invoke();
        }

        public async Task SupprimerAnnonce()
        {
            var error = await _annonceService.DeleteAsync(Annonce.IdAnnonce);

            if (error == null)
            {
                _notificationService.ShowSuccess(
                    "Suppression d'annonce",
                    "Votre annonce a bien été supprimée");

                IsOptionsMenuOpen = false;
                _nav.NavigateTo("/compte");
            }
            else
            {
                _notificationService.ShowError(
                    "Suppression d'annonce",
                    error);
            }

            _refreshUI?.Invoke();
        }

        private async Task LoadVendeurProfileImage(int idVendeur)
        {
            try
            {
                var img = await _imageService.GetImageProfil(idVendeur);

                if (img != null && img.Fichier != null && img.Fichier.Length > 0)
                {
                    var base64 = Convert.ToBase64String(img.Fichier);
                    ProfileImageSource = $"data:image/jpeg;base64,{base64}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement image profil vendeur: {ex.Message}");
            }
        }

        public async Task VoirProfilVendeur()
        {
            _nav.NavigateTo($"/comptepublic/{Annonce.IdVendeur}");
        }

        // ============================================================================
        // MÉTHODES IA - Prédiction de prix
        // ============================================================================

        public void CheckAndPredictPrice()
        {
            missingFields.Clear();

            if (Annonce == null) return;

            // Vérifier les champs requis (certains peuvent être manquants)
            if (string.IsNullOrEmpty(Annonce.Marque))
                missingFields.Add("Marque");
            if (string.IsNullOrEmpty(Annonce.Modele))
                missingFields.Add("Modèle");
            if (Annonce.Annee == 0)
                missingFields.Add("Année");
            if (string.IsNullOrEmpty(Annonce.Categorie))
                missingFields.Add("Catégorie");
            if (string.IsNullOrEmpty(Annonce.Carburant))
                missingFields.Add("Carburant");
            if (Annonce.Kilometrage == 0)
                missingFields.Add("Kilométrage");
            if (string.IsNullOrEmpty(Annonce.BoiteDeVitesse))
                missingFields.Add("Boîte de vitesse");
            if (string.IsNullOrEmpty(Annonce.Motricite))
                missingFields.Add("Motricité");

            if (missingFields.Any())
            {
                showPriceWarningPopup = true;
            }
            else
            {
                _ = ExecutePricePrediction();
            }

            _refreshUI?.Invoke();
        }

        public void ClosePriceWarningPopup()
        {
            showPriceWarningPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task ExecutePricePrediction()
        {
            if (Annonce == null) return;

            showPriceWarningPopup = false;
            showPriceLoadingPopup = true;
            _refreshUI?.Invoke();

            try
            {
                var dataPrediction = new DataPrediction()
                {
                    Manufacturer = Annonce.Marque ?? "Unknown",
                    Model = Annonce.Modele ?? "Unknown",
                    ProdYear = Annonce.Annee,
                    Category = IADataMapper.TranslateCategory(Annonce.Categorie ?? "Unknown"),
                    LeatherInterior = Annonce.InterieurCuire ? "Yes" : "No",
                    FuelType = IADataMapper.TranslateFuelType(Annonce.Carburant ?? "Unknown"),
                    EngineVolume = (float)Annonce.CylindrerMoteur,
                    Mileage = Annonce.Kilometrage.ToString(),
                    Cylinders = Annonce.NbCylindres > 0 ? (float)Annonce.NbCylindres : 4f,
                    GearBoxType = IADataMapper.TranslateGearBoxType(Annonce.BoiteDeVitesse ?? "Unknown"),
                    DriveWheels = IADataMapper.TranslateDriveWheels(Annonce.Motricite ?? "Unknown"),
                    Doors = Annonce.NbPortes.ToString(),
                    Wheel = Annonce.PositionVolant ? "Left wheel" : "Right wheel",
                    Color = IADataMapper.TranslateColor(Annonce.Couleur ?? "Black"),
                    Airbags = Annonce.NbAirbag,
                };

                var result = await _iaService.PredictAIAsync(dataPrediction);

                if (result is ResultatPrediction prediction)
                {
                    priceResult = prediction;
                }

                showPriceLoadingPopup = false;

                if (priceResult != null && priceResult.Success)
                {
                }
                else
                {
                    _notificationService.ShowError(
                        "Service indisponible",
                        "Le service d'IA est indisponible pour le moment"
                    );
                }
            }
            catch (Exception ex)
            {
                showPriceLoadingPopup = false;
                _notificationService.ShowError(
                    "Service indisponible",
                    "Le service d'IA est indisponible pour le moment"
                );
                Console.WriteLine($"Erreur prédiction prix: {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        // ============================================================================
        // MÉTHODES IA - Ajustement de prix
        // ============================================================================

        public bool CanUseAdjustment => 
            Annonce != null && 
            !string.IsNullOrWhiteSpace(Annonce.Description) && 
            Annonce.Prix > 0;

        public void OpenAdjustmentWarningPopup()
        {
            if (!CanUseAdjustment) return;
            showAdjustmentWarningPopup = true;
            _refreshUI?.Invoke();
        }

        public void CloseAdjustmentWarningPopup()
        {
            showAdjustmentWarningPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task ExecuteAdjustment()
        {
            if (Annonce == null) return;

            showAdjustmentWarningPopup = false;
            showAdjustmentLoadingPopup = true;
            _refreshUI?.Invoke();

            try
            {
                var dataAdjustment = new DataAjustement
                {
                    BasePrice = priceResult.PredictedPrice,
                    Description = Annonce.Description ?? "",
                };

                var result = await _iaService.PredictAIAsync(dataAdjustment);

                if (result is ResultatAjustement prediction)
                {
                    adjustmentResult = prediction;
                }

                showAdjustmentLoadingPopup = false;

                // ✅ VÉRIFICATION DU SUCCESS
                if (adjustmentResult != null && adjustmentResult.Success)
                {
                    showAdjustmentResultPopup = true;
                }
                else
                {
                    _notificationService.ShowError(
                        "Service indisponible",
                        "Le service d'IA est indisponible pour le moment"
                    );
                }
            }
            catch (Exception ex)
            {
                showAdjustmentLoadingPopup = false;
                _notificationService.ShowError(
                    "Service indisponible",
                    "Le service d'IA est indisponible pour le moment"
                );
                Console.WriteLine($"Erreur ajustement prix: {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        public void CloseAdjustmentResultPopup()
        {
            showAdjustmentResultPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task DisposeAsync()
        {
            if (_jsRuntime != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("car3DViewer.dispose");
                }
                catch
                {

                }
            }
        }
        
        public void RedirectToModif()
        {
            _nav.NavigateTo(modifAnnonceUrl);
        }
        
        public void OpenContactPopup()
        {
            if (!CurrentUserId.HasValue)
            {
                _nav.NavigateTo("/connexion");
                return;
            }
    
            showContactPopup = true;
            contactError = "";
            _refreshUI?.Invoke();
        }

        public void CloseContactPopup()
        {
            showContactPopup = false;
            contactError = "";
            _refreshUI?.Invoke();
        }

        public void UpdateContactMessage(string message)
        {
            contactMessage = message;
        }


        public IEnumerable<AnnonceDTO> GetVisibleSimilarAds()
        {
            return AnnonceSimilaires ?? Enumerable.Empty<AnnonceDTO>();
        }

        public async Task ExistConv()
        {
            ConvExistante = await _iaPourConversationService.ConvExist((int)CurrentUserId, Annonce.IdVendeur, Annonce.IdAnnonce);
        }

        public async Task RedirectionConversation()
        {
            _nav.NavigateTo("/conversations");
        }

        /// <summary>
        /// Calcule le décalage CSS pour le défilement
        /// Chaque carte fait environ 20% de largeur (100% / 5)
        /// </summary>
        public string GetCarouselTransform()
        {
            var cardWidth = 260;
            var gap = 20;
            var offset = CurrentSimilarIndex * (cardWidth + gap);
    
            return $"translateX(-{offset}px)";
        }

        /// <summary>
        /// Défile d'une carte vers la gauche
        /// </summary>
        public void PreviousSimilar()
        {
            if (CanGoPreviousSimilar)
            {
                CurrentSimilarIndex--;
                _refreshUI?.Invoke();
            }
        }

        /// <summary>
        /// Défile d'une carte vers la droite
        /// </summary>
        public void NextSimilar()
        {
            if (CanGoNextSimilar)
            {
                CurrentSimilarIndex++;
                _refreshUI?.Invoke();
            }
        }

        /// <summary>
        /// Va directement à une position spécifique
        /// </summary>
        public void GoToSimilarPage(int pageIndex)
        {
            if (pageIndex >= 0 && pageIndex < TotalSimilarPages)
            {
                CurrentSimilarIndex = pageIndex;
                _refreshUI?.Invoke();
            }
        }
        public void NavigateToSimilarAd(int idAnnonce)
        {
            _nav?.NavigateTo($"/annonce/{idAnnonce}");
        }
        public string GetSimilarAdImage(int idVoiture)
        {
            try
            {
                return _imageService.GetFirstImage(idVoiture);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement image similaire voiture {idVoiture}: {ex.Message}");
                return "https://via.placeholder.com/300x200?text=Pas+d%27image";
            }
        }

        public void Reset()
        {
            CurrentImageIndex = 0;
            IsLoading = true;
            IsFavorite = false;
            show3DViewer = false;
            isLoading3D = false;
            IsOptionsMenuOpen = false;
            infoOptionAnnonce = null;

            CurrentSimilarIndex = 0;
            AnnonceSimilaires = Enumerable.Empty<AnnonceDTO>();
        }

        //OFFRES
        public void ToggleOffreMode()
        {
            showOffreMode = !showOffreMode;
            if (!showOffreMode)
            {
                offreAmount = 0;
                offreError = "";
            }
            _refreshUI?.Invoke();
        }

        public void UpdateOffreAmount(decimal amount)
        {
            offreAmount = amount;
            offreError = "";
            _refreshUI?.Invoke();
        }

        public async Task SendContactMessageWithOffre()
        {
            if (!CurrentUserId.HasValue || Annonce == null)
            {
                contactError = "Vous devez être connecté pour envoyer un message";
                _refreshUI?.Invoke();
                return;
            }

            if (string.IsNullOrWhiteSpace(contactMessage))
            {
                contactError = "Le message ne peut pas être vide";
                _refreshUI?.Invoke();
                return;
            }

            if (showOffreMode)
            {
                if (offreAmount <= 0)
                {
                    offreError = "Le montant de l'offre doit être supérieur à 0";
                    _refreshUI?.Invoke();
                    return;
                }

                if (offreAmount > Annonce.Prix)
                {
                    offreError = "Le montant de l'offre ne peut pas être supérieur au prix de l'annonce";
                    _refreshUI?.Invoke();
                    return;
                }
            }

            isLoadingContact = true;
            contactError = "";
            offreError = "";
            _refreshUI?.Invoke();

            try
            {
                var conversationDto = new ConversationCreateDTO
                {
                    IdAnnonce = Annonce.IdAnnonce,
                    message = contactMessage,
                    DateDernierMessage = DateTime.Now
                };

                var conversation = await _conversationService.PostComplet(
                    conversationDto,
                    CurrentUserId.Value,
                    Annonce.IdVendeur
                );

                if (showOffreMode && conversation != null)
                {
                    // 🔍 SOLUTION : Récupérer les messages de la conversation pour trouver le dernier
                    var messages = await _messageService.GetMessagesByConversationAndMarkAsRead(
                        conversation.IdConversation,
                        CurrentUserId.Value
                    );

                    var lastMessage = messages.OrderByDescending(m => m.DateEnvoiMessage).FirstOrDefault();

                    if (lastMessage != null)
                    {
                        // ✅ Créer l'offre liée au message EXISTANT
                        var offreDto = new OffreCreateDTO
                        {
                            IdAnnonce = Annonce.IdAnnonce,
                            IdMessage = lastMessage.IdMessage,
                            Valeur = offreAmount
                        };

                        await _offreService.CreateAsync(offreDto);

                        Console.WriteLine($"✅ Offre de {offreAmount:N0} € créée pour message {lastMessage.IdMessage}");
                    }
                    else
                    {
                        Console.WriteLine("❌ Impossible de trouver le message créé");
                    }
                }

                await _conversationStateService.ReloadConversationsAsync();
                Console.WriteLine("✅ Conversations rechargées après création");

                _notificationService.ShowSuccess(
                    showOffreMode ? "Offre envoyée" : "Message envoyé",
                    showOffreMode
                        ? $"Votre offre de {offreAmount:N0} € a été envoyée au vendeur"
                        : "Votre message a été envoyé au vendeur avec succès"
                );

                // ✅ Reset et fermeture
                contactMessage = "";
                offreAmount = 0;
                showOffreMode = false;
                showContactPopup = false;
            }
            catch (Exception ex)
            {
                contactError = "Erreur lors de l'envoi. Veuillez réessayer.";
                Console.WriteLine($"Erreur envoi message/offre: {ex.Message}");
            }
            finally
            {
                isLoadingContact = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task GetPaiements()
        {
            paiement = await _annonceService.GetPaiementByIdAnnonce(Annonce.IdAnnonce);
        }
    }
}
