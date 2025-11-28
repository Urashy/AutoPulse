using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Threading.Tasks;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceDetailViewModel
    {
        private readonly IAnnonceDetailService _annonceService;
        private readonly IPostImageService _imageService;
        private readonly IFavorisService _favorisService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public List<int> ImageIds { get; private set; } = new();
        public int CurrentImageIndex { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;

        private Action? _refreshUI;

        public AnnonceDetailViewModel(
            IAnnonceDetailService annonceService,
            IPostImageService imageService,
            IFavorisService favorisService)
        {
            _annonceService = annonceService;
            _imageService = imageService;
            _favorisService= favorisService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI)
        {
            _refreshUI = refreshUI;
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                // Charger l'annonce
                Annonce = await _annonceService.GetByIdAsync(idAnnonce);

                if (Annonce != null && Annonce.IdVoiture > 0)
                {
                    // Charger les IDs des images (à implémenter dans le service)
                    // Pour l'instant, on utilise juste l'image principale
                    ImageIds = new List<int> { Annonce.IdVoiture };
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

        public async Task AddFavorite(int idannonce, int idcompte)
        {
            Favori fav = new Favori()
            {
                IdAnnonce = idannonce,
                IdCompte = idcompte
            };
            await _favorisService.CreateAsync(fav);
        }
    }
}