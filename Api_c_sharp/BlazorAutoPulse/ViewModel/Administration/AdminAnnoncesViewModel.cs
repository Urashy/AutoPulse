using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminAnnoncesViewModel
    {
        private readonly IAnnonceService _annonceService;

        // On utilise AnnonceDTO pour être compatible avec AnnonceComposant
        private List<AnnonceDTO> AllAnnonces { get; set; } = new();
        public List<AnnonceDTO> FilteredAnnonces { get; private set; } = new();

        public string SearchQuery { get; set; } = "";

        // Pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 12; // Modifiable
        public int TotalPages => FilteredAnnonces.Count == 0 ? 1 : (int)Math.Ceiling((double)FilteredAnnonces.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;
        public string PaginationInfo => $"Page {CurrentPage} / {TotalPages} ({FilteredAnnonces.Count} annonces)";

        public bool IsLoading { get; private set; } = true;

        private Action? _refreshUI;

        public AdminAnnoncesViewModel(IAnnonceService annonceService)
        {
            _annonceService = annonceService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadAnnonces();
        }

        private async Task LoadAnnonces()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                var result = await _annonceService.GetAllAsync();
                AllAnnonces = result.ToList();
                FilteredAnnonces = AllAnnonces;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public List<AnnonceDTO> GetPagedAnnonces()
        {
            return FilteredAnnonces
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();
        }

        public void SearchAnnonces()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                FilteredAnnonces = AllAnnonces;
            }
            else
            {
                var q = SearchQuery.ToLower();
                FilteredAnnonces = AllAnnonces.Where(a =>
                    (a.Modele != null && a.Modele.ToLower().Contains(q)) ||
                    (a.Marque != null && a.Marque.ToLower().Contains(q))
                ).ToList();
            }
            CurrentPage = 1;
            _refreshUI?.Invoke();
        }

        public async Task DeleteAnnonce(AnnonceDTO annonce)
        {
            try
            {
                await _annonceService.DeleteAsync(annonce.IdAnnonce); // Assure-toi que DeleteAsync prend l'ID ou l'objet
                AllAnnonces.Remove(annonce);
                SearchAnnonces(); // Rafraichir le filtre
                Console.WriteLine($"Annonce {annonce.IdAnnonce} supprimée");
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur suppression: {ex.Message}");
            }
        }

        public void NextPage() { if (CanGoNext) { CurrentPage++; _refreshUI?.Invoke(); } }
        public void PreviousPage() { if (CanGoPrevious) { CurrentPage--; _refreshUI?.Invoke(); } }
    }
}