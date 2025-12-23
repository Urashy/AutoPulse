using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Services;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceComposantViewModel
    {
        private readonly IImageService _imageService;
        private readonly ICompteService _compteService;
        private readonly FavoriStateService _favorisStateService;

        public bool IsFavorite { get; private set; }
        private int? _currentUserId;
        private Action? _refreshUI;
        private int _currentAnnonceId;

        public AnnonceComposantViewModel(
            IImageService imageService,
            ICompteService compteService,
            FavoriStateService favorisStateService)
        {
            _imageService = imageService;
            _compteService = compteService;
            _favorisStateService = favorisStateService;
        }

        public async Task InitializeAsync(AnnonceDTO annonce, Action refreshUI)
        {
            _refreshUI = refreshUI;
            _currentAnnonceId = annonce.IdAnnonce;

            try
            {
                var compte = await _compteService.GetMe();
                _currentUserId = compte?.IdCompte;

                if (_currentUserId.HasValue && annonce != null)
                {
                    // ✅ Utilisation du FavorisStateService pour vérifier le statut
                    IsFavorite = _favorisStateService.IsFavorite(annonce.IdAnnonce);
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
                // ✅ Utilisation du FavorisStateService qui gère automatiquement SignalR
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
