using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{

    public class HomeViewModel
    {
        private readonly IAnnonceService _annonceService;

        // Pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 21;
        private bool HasMorePages { get; set; } = true;

        // Données
        public AnnonceDTO[] allAnnonces { get; private set; } = Array.Empty<AnnonceDTO>();
        public bool IsLoading { get; private set; } = true;

        // Propriétés calculées pour la pagination
        public int CurrentPageResultCount => allAnnonces?.Length ?? 0;
        public string PaginationInfo => $"Page {CurrentPage} - {CurrentPageResultCount} résultat(s)";
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => HasMorePages && CurrentPageResultCount == ItemsPerPage;
        public bool ShowPagination => CurrentPage > 1 || CanGoToNextPage;

        private Action? _refreshUI;

        public HomeViewModel(IAnnonceService annonceService)
        {
            _annonceService = annonceService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadPage();
        }

        private async Task LoadPage()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                // ✅ Appel direct avec pagination côté API
                var results = await _annonceService.GetByIdMiseEnAvant(3, CurrentPage, ItemsPerPage);
                allAnnonces = results.ToArray();

                // Si on reçoit moins de résultats que demandé, c'est qu'il n'y a plus de pages
                HasMorePages = allAnnonces.Length == ItemsPerPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement: {ex.Message}");
                allAnnonces = Array.Empty<AnnonceDTO>();
                HasMorePages = false;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task GoToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber != CurrentPage)
            {
                CurrentPage = pageNumber;
                await LoadPage();
            }
        }

        public async Task GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
                await LoadPage();
            }
        }

        public async Task GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
                await LoadPage();
            }
        }

        public PaginationData GetPaginationData()
        {
            var estimatedTotalPages = CalculateEstimatedTotalPages();

            return new PaginationData
            {
                CurrentPage = CurrentPage,
                TotalPages = estimatedTotalPages,
                TotalResults = CurrentPageResultCount,
                CanGoToPrevious = CanGoToPreviousPage,
                CanGoToNext = CanGoToNextPage,
                ShowFirstPage = CurrentPage > 3,
                ShowLastPage = false,
                ShowFirstDots = CurrentPage > 4,
                ShowLastDots = CanGoToNextPage && CurrentPage > 2,
                VisiblePages = GetVisiblePageNumbers()
            };
        }

        private int CalculateEstimatedTotalPages()
        {
            if (allAnnonces == null || allAnnonces.Length == 0) return 1;
            if (!HasMorePages && CurrentPage == 1) return 1;
            if (!HasMorePages) return CurrentPage;
            return CurrentPage + 1;
        }

        private int[] GetVisiblePageNumbers()
        {
            var pages = new List<int>();
            var startPage = Math.Max(1, CurrentPage - 2);
            var endPage = CurrentPage + 2;

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages.ToArray();
        }
    }
}