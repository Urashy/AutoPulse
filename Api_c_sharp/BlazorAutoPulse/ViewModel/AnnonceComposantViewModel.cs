using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System.Threading.Tasks;

namespace BlazorAutoPulse.ViewModel
{
    public class AnnonceComposantViewModel
    {
        private readonly IFavorisService _favorisService;
        private Annonce _annonce;
        private bool _isFavorite;

        // [ATTENTION]: Placeholder pour l'ID du compte. 
        // En production, cette valeur devrait être récupérée via un service d'authentification ou le CascadingParameter.
        private const int CurrentCompteId = 1;

        public AnnonceComposantViewModel(IFavorisService favorisService)
        {
            _favorisService = favorisService;
        }

        // La propriété Annonce est définie par le composant. Son setter déclenche le chargement de l'état.
        public Annonce Annonce
        {
            get => _annonce;
            set
            {
                _annonce = value;
                // Le composant appellera LoadFavoriteStatus() dans OnInitialized pour le chargement initial.
            }
        }

        // Propriété d'état observable pour le composant
        public bool IsFavorite
        {
            get => _isFavorite;
            set => _isFavorite = value;
        }

        public string GetFirstImage(int idVoiture)
        {
            
            return $"/api/Image/first/{idVoiture}";
        }

        public async Task LoadFavoriteStatus()
        {
            if (CurrentCompteId > 0 && Annonce != null && Annonce.IdAnnonce > 0)
            {
                IsFavorite = await _favorisService.IsFavorite(CurrentCompteId, Annonce.IdAnnonce);
            }
        }

        public async Task ToggleFavoriteStatus()
        {
            if (CurrentCompteId > 0 && Annonce != null && Annonce.IdAnnonce > 0)
            {
                // La méthode ToggleFavorite du service gère l'ajout/suppression et retourne le nouvel état
                IsFavorite = await _favorisService.ToggleFavorite(CurrentCompteId, Annonce.IdAnnonce);
            }
        }
    }
}