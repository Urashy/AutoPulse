using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using BlazorAutoPulse.Service;
using Microsoft.AspNetCore.Components;

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
        private readonly NotificationService  _notificationService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public List<int> ImageIds { get; private set; } = new();
        public int CurrentImageIndex { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;
        public bool IsFavorite { get; private set; } = false;
        public int? CurrentUserId { get; private set; }

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
            NotificationService notificationService)
        {
            _annonceService = annonceService;
            _postImageService = postImageService;
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
            _couleurService = couleurService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI, IJSRuntime jsRuntime,  NavigationManager nav)
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
                }
                
                couleurDisponible = await _couleurService.GetCouleursByVoitureId(Annonce.IdVoiture);
                selectedColor = couleurDisponible?.FirstOrDefault()?.CodeHexaCouleur;

                if (Annonce != null && Annonce.IdVoiture > 0)
                {
                    ImageIds = new List<int> { Annonce.IdVoiture };

                    // Vérifier si l'annonce est en favoris
                    if (CurrentUserId.HasValue)
                    {
                        IsFavorite = await _favorisService.IsFavorite(CurrentUserId.Value, idAnnonce);
                    }
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
                        await Task.Delay(500); // Petit délai pour s'assurer que le DOM est prêt
                        await Initialize3DViewer();
                    });
                }
            }
            
            EstMasquerAnnonce();
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

        // Méthodes pour les images
        public string GetCurrentImage()
        {
            if (CurrentImageIndex >= 0 && CurrentImageIndex < ImageIds.Count)
            {
                return _imageService.GetFirstImage(ImageIds[CurrentImageIndex]);
            }
            return "";
        }

        public string GetImageUrl(int imageId)
        {
            return _imageService.GetFirstImage(imageId);
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
            
            // Si on active le 3D et qu'il n'est pas encore chargé
            if (show3DViewer && !is3DReady)
            {
                isLoading3D = true;
                _refreshUI?.Invoke();
                
                await Task.Delay(300); // Attendre que le DOM soit prêt
                await Initialize3DViewer();
            }
        }

        private async Task Preload3DViewer()
        {
            if (_jsRuntime == null || Annonce == null)
            {
                Console.WriteLine("JSRuntime ou Annonce est null");
                return;
            }
            
            if (string.IsNullOrEmpty(Annonce.LienModeleBlender))
            {
                Console.WriteLine("Pas de lien vers le modèle 3D");
                return;
            }

            try
            {
                Console.WriteLine("Préchargement du modèle 3D en arrière-plan...");
                
                // Vérifier que le conteneur existe (créé même si pas visible)
                await EnsureHiddenContainerExists();
                
                await _jsRuntime.InvokeVoidAsync("car3DViewer.init", "car3DViewer", Annonce.LienModeleBlender);
                
                is3DReady = true;
                Console.WriteLine("Modèle 3D préchargé avec succès (en arrière-plan)");
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du préchargement du modèle 3D: {ex.Message}");
            }
        }

        private async Task EnsureHiddenContainerExists()
        {
            // Créer temporairement le conteneur s'il n'existe pas
            var containerExists = await _jsRuntime.InvokeAsync<bool>("eval",
                "document.getElementById('car3DViewer') !== null");

            if (!containerExists)
            {
                // Le conteneur sera créé lors de l'affichage
                await Task.Delay(100);
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
                
                // Vérifier que le conteneur existe
                var containerExists = await _jsRuntime.InvokeAsync<bool>("eval", 
                    "document.getElementById('car3DViewer') !== null");
                
                Console.WriteLine($"Le conteneur existe: {containerExists}");
                
                if (!containerExists)
                {
                    Console.WriteLine("Le conteneur n'existe pas encore, on réessaie dans 500ms...");
                    await Task.Delay(500);
                    
                    // Deuxième tentative
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
                
                // Récupérer les animations disponibles
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
            AnnonceCreateDTO annonceChange = new AnnonceCreateDTO()
            {
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
            AnnonceCreateDTO annonceChange = new AnnonceCreateDTO()
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