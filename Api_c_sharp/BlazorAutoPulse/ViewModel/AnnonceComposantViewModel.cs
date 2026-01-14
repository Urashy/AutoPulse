using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Services;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceComposantViewModel
    {
        private readonly IImageService _imageService;
        private readonly IFavorisService _favorisService;
        private readonly FavoriStateService _favorisStateService;

        public bool IsFavorite { get; private set; }
        private int? _currentUserId;
        private Action? _refreshUI;
        private int _currentAnnonceId;

        public AnnonceComposantViewModel(
            IImageService imageService,
            IFavorisService favorisService,
            FavoriStateService favorisStateService)
        {
            _imageService = imageService;
            _favorisService = favorisService;
            _favorisStateService = favorisStateService;
        }

        public async Task InitializeAsync(AnnonceDTO annonce, int? idCompte, Action refreshUI)
        {
            _refreshUI = refreshUI;
            _currentAnnonceId = annonce.IdAnnonce;
            _currentUserId = idCompte;
            IsFavorite = false;

            if (_currentUserId.HasValue && annonce != null)
            {
                IsFavorite = await _favorisService.IsFavorite((int)_currentUserId, _currentAnnonceId);
            }
        }

        public string GetFirstImage(int idVoiture)
        {
            try
            {
                return _imageService.GetFirstImage(idVoiture);
            }
            catch
            {
                return "https://via.placeholder.com/300x200?text=No+Image";
            }
        }

        public async Task ToggleFavoriteStatus(int idannonce)
        {
            if (!_currentUserId.HasValue) return;
            try
            {
                bool newStatus = await _favorisStateService.ToggleFavorisAsync(idannonce);
                IsFavorite = newStatus;
                
                Console.WriteLine($"✅ Favori toggled pour annonce {idannonce}: {IsFavorite}");
                
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur toggle favori: {ex.Message}");
            }
        }
    }
}
