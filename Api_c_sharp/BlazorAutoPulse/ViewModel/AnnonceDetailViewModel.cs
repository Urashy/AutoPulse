using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using BlazorAutoPulse.Service;
using Microsoft.AspNetCore.Components;
using BlazorAutoPulse.Service.WebService;

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

        public AnnonceDetailDTO? Annonce { get; private set; }
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

        public string? erreurSupprimeAnnonce = null;
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
            NotificationService notificationService)
        {
            _annonceService = annonceService;
            _postImageService = postImageService;
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
            _couleurService = couleurService;
            _notificationService = notificationService;
            _conversationService = conversationService;
            _vueService = vueService;
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

                // Charger l'annonce
                Annonce = await _annonceService.GetAnnonceDetailById(idAnnonce);
            
                if (Annonce != null)
                {
                    await LoadVendeurProfileImage(Annonce.IdVendeur);
                    await LoadAllImages();
                    modifAnnonceUrl = $"/modifier-annonce/{Annonce.IdAnnonce}";
                }

                

                couleurDisponible = await _couleurService.GetCouleursByVoitureId(Annonce.IdVoiture);
                selectedColor = couleurDisponible?.FirstOrDefault()?.CodeHexaCouleur;

                // Vérifier si l'annonce est en favoris
                if (Annonce != null && CurrentUserId.HasValue)
                {
                    IsFavorite = await _favorisService.IsFavorite(CurrentUserId.Value, idAnnonce);

                    await _vueService.CreateAsync(new VueDTO
                    {
                        IdAnnonce = idAnnonce,
                        IdCompte = CurrentUserId ?? 0
                    });
                }
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
                IsFavorite = await _favorisService.ToggleFavorite(CurrentUserId.Value, Annonce.IdAnnonce);
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout/suppression du favori: {ex.Message}");
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
            _annonceService.UpdateAnnonceAsync(Annonce.IdAnnonce, annonceChange);
            IsOptionsMenuOpen = false;

            await EstMasquerAnnonce();
            _refreshUI?.Invoke();
        }

        public async Task EstMasquerAnnonce()
        {
            estMasquer = await _annonceService.EstMasquerAsync(Annonce.IdAnnonce);
            _refreshUI?.Invoke();
        }

        public async void SupprimerAnnonce()
        {
            try
            {
                await _annonceService.DeleteAsync(Annonce.IdAnnonce);
                _notificationService.ShowSuccess(
                    "Suppression d'annonce",
                    "Votre annonce a bien été supprimée");
                IsOptionsMenuOpen = false;
                _nav.NavigateTo("/compte");
            }
            catch
            {
                erreurSupprimeAnnonce = "L'annonce n'a pas pu être supprimée";
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
                    Category = Annonce.Categorie ?? "Unknown",
                    LeatherInterior = "No",
                    FuelType = Annonce.Carburant ?? "Unknown",
                    EngineVolume = 2.0f,
                    Mileage = Annonce.Kilometrage.ToString(),
                    Cylinders = Annonce.NbCylindres > 0 ? (float)Annonce.NbCylindres : 4f,
                    GearBoxType = Annonce.BoiteDeVitesse ?? "Unknown",
                    DriveWheels = Annonce.Motricite ?? "Unknown",
                    Doors = "4",
                    Wheel = "Left wheel",
                    Color = Annonce.Couleur ?? "Black",
                    Airbags = 4,
                    Levy = 0f
                };

                var result = await _iaService.PredictAIAsync(dataPrediction);
                Console.WriteLine($"Type reçu: {result?.GetType().Name}");

                if (result is ResultatAjustement prediction)
                {
                    adjustmentResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatAjustement");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatAjustement");
                }

                showPriceLoadingPopup = false;

                if (priceResult.Success)
                {
                    _notificationService.ShowSuccess(
                        "Prédiction du prix",
                        $"Prix estimé : {priceResult.PredictedPrice:N0} {priceResult.Currency}"
                    );
                }
                else
                {
                    _notificationService.ShowError(
                        "Erreur de prédiction",
                        priceResult.Error ?? "Une erreur est survenue"
                    );
                }
            }
            catch (Exception ex)
            {
                showPriceLoadingPopup = false;
                _notificationService.ShowError(
                    "Erreur de prédiction",
                    $"Erreur: {ex.Message}"
                );
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
                Console.WriteLine($"Type reçu: {result?.GetType().Name}");

                if (result is ResultatPrediction prediction)
                {
                    priceResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatPrediction");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatPrediction");
                }

                showAdjustmentLoadingPopup = false;

                if (adjustmentResult.Success)
                {
                    showAdjustmentResultPopup = true;
                }
                else
                {
                    _notificationService.ShowError(
                        "Erreur d'ajustement",
                        adjustmentResult.Error ?? "Une erreur est survenue"
                    );
                }
            }
            catch (Exception ex)
            {
                showAdjustmentLoadingPopup = false;
                _notificationService.ShowError(
                    "Erreur d'ajustement",
                    $"Erreur: Veuillez prédire le prix avant"
                );
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

        public async Task SendContactMessage()
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

            isLoadingContact = true;
            contactError = "";
            _refreshUI?.Invoke();

            try
            {
                var conversationDto = new ConversationCreateDTO
                {
                    IdAnnonce = Annonce.IdAnnonce,
                    message = contactMessage,
                    DateDernierMessage = DateTime.Now
                };
                
                await _conversationService.PostComplet(
                    conversationDto, 
                    CurrentUserId.Value, 
                    Annonce.IdVendeur
                );

                // Succès : fermer la popup et vider le message
                _notificationService.ShowSuccess(
                    "Message envoyé",
                    "Votre message a été envoyé au vendeur avec succès"
                );
        
                contactMessage = "";
                showContactPopup = false;
            }
            catch (Exception ex)
            {
                contactError = "Erreur lors de l'envoi du message. Veuillez réessayer.";
                Console.WriteLine($"Erreur envoi message: {ex.Message}");
            }
            finally
            {
                isLoadingContact = false;
                _refreshUI?.Invoke();
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
        }
    }
}