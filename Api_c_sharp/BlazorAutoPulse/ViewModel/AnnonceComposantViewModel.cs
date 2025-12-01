using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceComposantViewModel
    {
        private readonly IFavorisService _favorisService;
        private readonly ICompteService _compteService;
        private readonly IImageService _imageService;

        public Annonce Annonce { get; set; }
        public bool IsFavorite { get; private set; }
        private int? CurrentUserId { get; set; }
        private Action? _refreshUI;

        public AnnonceComposantViewModel(
            IFavorisService favorisService,
            ICompteService compteService,
            IImageService imageService)
        {
            _favorisService = favorisService;
            _compteService = compteService;
            _imageService = imageService;
        }

        public async Task InitializeAsync(Annonce annonce, Action refreshUI)
        {
            Annonce = annonce;
            _refreshUI = refreshUI;

            try
            {
                var compte = await _compteService.GetMe();
                CurrentUserId = compte?.IdCompte;

                if (CurrentUserId.HasValue && Annonce != null)
                {
                    await LoadFavoriteStatus();
                }
            }
            catch
            {
                CurrentUserId = null;
                IsFavorite = false;
            }
        }

        public async Task LoadFavoriteStatus()
        {
            if (CurrentUserId.HasValue && Annonce != null)
            {
                try
                {
                    IsFavorite = await _favorisService.IsFavorite(CurrentUserId.Value, Annonce.IdAnnonce);
                    _refreshUI?.Invoke();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du chargement du statut favori: {ex.Message}");
                    IsFavorite = false;
                }
            }
        }

        public async Task ToggleFavoriteStatus()
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
                Console.WriteLine($"Erreur lors du toggle favori: {ex.Message}");
            }
        }

        public string GetFirstImage(int idVoiture)
        {
            return _imageService.GetFirstImage(idVoiture);
        }
    }
}