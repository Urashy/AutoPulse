using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceDetailViewModel
    {
        private readonly IAnnonceDetailService _annonceService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly IFavorisService _favorisService;
        private readonly ICompteService _compteService;
        private readonly ICouleurService _couleurService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public List<int> ImageIds { get; private set; } = new();
        public int CurrentImageIndex { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;
        public bool IsFavorite { get; private set; } = false;
        public int? CurrentUserId { get; private set; }

        // Propriétés pour le visualiseur 3D
        public bool show3DViewer { get; private set; } = false;
        public bool isLoading3D { get; private set; } = false;
        public List<Couleur> couleurDisponible { get; set; }
        public string selectedColor { get; private set; }

        private Action? _refreshUI;
        private IJSRuntime? _jsRuntime;

        public AnnonceDetailViewModel(
            IAnnonceDetailService annonceService,
            IPostImageService postImageService,
            IFavorisService favorisService,
            ICompteService compteService,
            IImageService imageService,
            ICouleurService couleurService)
        {
            _annonceService = annonceService;
            _postImageService = postImageService;
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
            _couleurService = couleurService;
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
                    Compte user = await _compteService.GetMe();
                    CurrentUserId = user.IdCompte;
                }
                catch
                {
                    CurrentUserId = null;
                }

                // Charger l'annonce
                Annonce = await _annonceService.GetByIdAsync(idAnnonce);
                
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
            
            if (show3DViewer)
            {
                isLoading3D = true;
                _refreshUI?.Invoke();
                
                // Attendre que le DOM soit mis à jour
                await Task.Delay(300);
                
                // Initialiser directement
                await Initialize3DViewer();
            }
            else
            {
                _refreshUI?.Invoke();
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

                Console.WriteLine($"Chargement du modèle: {Annonce.LienModeleBlender}");
                
                await _jsRuntime.InvokeVoidAsync("car3DViewer.init", "car3DViewer", Annonce.LienModeleBlender);
                
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