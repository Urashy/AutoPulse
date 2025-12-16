using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminAnnoncesViewModel
    {
        private readonly IAnnonceService _annonceService;

        public List<AnnonceDTO> FilteredAnnonces { get; private set; } = new();

        public string SearchQuery { get; set; } = "";

        // Pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 12;

        public bool HasMorePages { get; private set; }

        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => HasMorePages;
        public string PaginationInfo => $"Page {CurrentPage} - {FilteredAnnonces.Count} affichée(s)";

        public int TotalPages => HasMorePages ? CurrentPage + 1 : CurrentPage;

        public bool IsLoading { get; private set; } = true;

        private Action? _refreshUI;

        public AdminAnnoncesViewModel(IAnnonceService annonceService)
        {
            _annonceService = annonceService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await PerformSearch();
        }

        private async Task PerformSearch()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                var searchParams = new ParametreRecherche
                {
                    Nom = SearchQuery ?? string.Empty, // Recherche textuelle
                    PageNumber = CurrentPage,
                    PageSize = ItemsPerPage,
                    PrixMin = 0,
                    PrixMax = 0, // 0 = infini dans ta logique généralement
                    KmMin = 0,
                    KmMax = 0,
                    IdMarque = 0,
                    IdCarburant = 0,
                    IdTypeVoiture = 0 // Categorie
                    ,Order = 3
                };

                var result = await _annonceService.GetFilteredAnnoncesAsync(searchParams);

                FilteredAnnonces = result.ToList();

                HasMorePages = FilteredAnnonces.Count == ItemsPerPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement annonces : {ex.Message}");
                FilteredAnnonces = new List<AnnonceDTO>();
                HasMorePages = false;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public List<AnnonceDTO> GetPagedAnnonces()
        {
            return FilteredAnnonces;
        }

        public async Task SearchAnnonces()
        {
            CurrentPage = 1;
            await PerformSearch();
        }

        public async Task DeleteAnnonce(AnnonceDTO annonce)
        {
            try
            {
                await _annonceService.DeleteAsync(annonce.IdAnnonce); // Utilise IdAnnonce ou Id selon ton DTO

                // On retire l'élément de la liste locale pour un feedback immédiat
                FilteredAnnonces.Remove(annonce);

                // Optionnel : Recharger la page pour combler le trou s'il y a d'autres éléments
                // await PerformSearch(); 

                Console.WriteLine($"Annonce {annonce.IdAnnonce} supprimée");
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression : {ex.Message}");
            }
        }

        // Navigation Async
        public async Task NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                await PerformSearch();
            }
        }

        public async Task PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                await PerformSearch();
            }
        }
    }
}