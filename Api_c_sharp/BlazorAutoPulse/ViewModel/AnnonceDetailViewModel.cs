using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
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

        public AnnonceDetailDTO? Annonce { get; private set; }
        public List<int> ImageIds { get; private set; } = new();
        public int CurrentImageIndex { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;
        public bool IsFavorite { get; private set; } = false;
        public int? CurrentUserId { get; private set; }

        private Action? _refreshUI;

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

        public async Task InitializeAsync(int idAnnonce, Action refreshUI)
        {
            _refreshUI = refreshUI;
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
    }
}