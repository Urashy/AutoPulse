using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceComposantViewModel
    {
        private readonly IImageService _imageService;
        private readonly IFavorisService _favorisService;
        private readonly ICompteService _compteService;

        public bool IsFavorite { get; private set; }
        private int? _currentUserId;
        private Action? _refreshUI;

        public AnnonceComposantViewModel(
            IImageService imageService,
            IFavorisService favorisService,
            ICompteService compteService)
        {
            _imageService = imageService;
            _favorisService = favorisService;
            _compteService = compteService;
        }

        public async Task InitializeAsync(AnnonceDTO annonce, Action refreshUI)
        {
            _refreshUI = refreshUI;

            try
            {
                var compte = await _compteService.GetMe();
                _currentUserId = compte?.IdCompte;

                if (_currentUserId.HasValue && annonce != null)
                {
                    IsFavorite = await _favorisService.IsFavorite(_currentUserId.Value, annonce.IdAnnonce);
                }
            }
            catch
            {
                _currentUserId = null;
                IsFavorite = false;
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
                IsFavorite = !IsFavorite;
                _favorisService.ToggleFavorite(_currentUserId.Value, idannonce); 
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur toggle favori: {ex.Message}");
                IsFavorite = !IsFavorite;
                _refreshUI?.Invoke();
            }
        }
    }
}