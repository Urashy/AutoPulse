using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using AnnonceDetailDTO = BlazorAutoPulse.Model.AnnonceDetailDTO;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceDetailViewModel
    {
        private readonly IAnnonceDetailService _annonceService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly IFavorisService _favorisService;
        private readonly ICompteService _compteService;

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
        public string selectedColor { get; private set; } = "#ff0000";
        public List<string> availableAnimations { get; private set; } = new();
        
        public List<Couleur> availableColors { get; } = new()
        {
            new Couleur { LibelleCouleur = "Rouge", CodeHexaCouleur = "#ff0000" },
            new Couleur { LibelleCouleur = "Bleu", CodeHexaCouleur = "#0066ff" },
            new Couleur { LibelleCouleur = "Noir", CodeHexaCouleur = "#1a1a1a" },
            new Couleur { LibelleCouleur = "Blanc", CodeHexaCouleur = "#ffffff" },
            new Couleur { LibelleCouleur = "Gris", CodeHexaCouleur = "#808080" },
            new Couleur { LibelleCouleur = "Argent", CodeHexaCouleur = "#c0c0c0" },
            new Couleur { LibelleCouleur = "Jaune", CodeHexaCouleur = "#ffdd00" },
            new Couleur { LibelleCouleur = "Vert", CodeHexaCouleur = "#00aa44" },
            new Couleur { LibelleCouleur = "Orange", CodeHexaCouleur = "#ff6600" }
        };

        private Action? _refreshUI;
        private IJSRuntime? _jsRuntime;

        public AnnonceDetailViewModel(
            IAnnonceDetailService annonceService,
            IPostImageService postImageService,
            IFavorisService favorisService,
            ICompteService compteService,
            IImageService imageService)
        {
            _annonceService = annonceService;
            _postImageService = postImageService;
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI, IJSRuntime jsRuntime)
        {
            _refreshUI = refreshUI;
            _jsRuntime = jsRuntime;
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
                Annonce = await _annonceService.GetByIdAsync(idAnnonce);

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
                
                // Récupérer les animations disponibles
                try
                {
                    var animations = await _jsRuntime.InvokeAsync<string[]>("car3DViewer.getAnimationNames");
                    availableAnimations = animations?.ToList() ?? new List<string>();
                    if (availableAnimations.Any())
                    {
                        Console.WriteLine($"Animations disponibles: {string.Join(", ", availableAnimations)}");
                    }
                }
                catch
                {
                    // Pas grave si on ne peut pas récupérer les animations
                }
                
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
                availableAnimations = animations?.ToList() ?? new List<string>();
                Console.WriteLine($"Animations disponibles: {string.Join(", ", availableAnimations)}");
                
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

        public async Task ToggleAnimation(string animationName)
        {
            if (_jsRuntime != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("car3DViewer.toggleAnimation", animationName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'animation: {ex.Message}");
                }
            }
        }

        public string GetAnimationLabel(string animName)
        {
            // Convertir les noms d'animation en labels lisibles
            return animName switch
            {
                "DoorFrontLeft" or "door_front_left" or "PorteAvantGauche" => "🚪 Porte avant gauche",
                "DoorFrontRight" or "door_front_right" or "PorteAvantDroite" => "🚪 Porte avant droite",
                "DoorRearLeft" or "door_rear_left" or "PorteArriereGauche" => "🚪 Porte arrière gauche",
                "DoorRearRight" or "door_rear_right" or "PorteArriereDroite" => "🚪 Porte arrière droite",
                "Hood" or "hood" or "Capot" => "🔧 Capot",
                "Trunk" or "trunk" or "Coffre" => "📦 Coffre",
                "Window" or "window" or "Vitre" => "🪟 Vitres",
                _ => animName // Nom par défaut si pas de correspondance
            };
        }

        public async Task DisposeAsync()
        {
            if (_jsRuntime != null)
            {
                try
                {
                    await _jsRuntime.InvokeVoidAsync("car3DViewer.dispose");
                }
                catch { }
            }
        }
        
        public void Reset()
        {
            CurrentImageIndex  = 0;
            IsLoading  = true;
            IsFavorite  = false;
            show3DViewer  = false;
            isLoading3D  = false;
        }
    }
}