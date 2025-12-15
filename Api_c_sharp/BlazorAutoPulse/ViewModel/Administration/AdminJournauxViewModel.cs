using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminJournauxViewModel
    {
        private readonly IJournalService _journalService;
        private Action? _refreshUI;

        public bool IsLoading { get; private set; } = true;
        public List<JournalDTO> PagedJournaux { get; private set; } = new();

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 15;
        public int TotalFilteredItems { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalFilteredItems / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        // --- FILTRES ---
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        // Filtre Type Journal (nullable pour "Tous")
        public int? SelectedTypeId { get; set; }

        // Tri : 1 = Croissant, 0 (ou autre) = Décroissant. 
        // Par défaut 0 pour avoir les logs récents en premier.
        public int SortOrder { get; private set; } = 0;

        public AdminJournauxViewModel(IJournalService journalService)
        {
            _journalService = journalService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadJournaux();
        }

        public async Task LoadJournaux()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            // Appel respectant l'ordre : typeId, dateDebut, dateFin, ordre
            var result = await _journalService.GetFilteredAsync(SelectedTypeId, DateDebut, DateFin, SortOrder);

            var allFiltered = result.ToList();
            TotalFilteredItems = allFiltered.Count;

            PagedJournaux = allFiltered
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            IsLoading = false;
            _refreshUI?.Invoke();
        }

        public async Task OnFilterChanged()
        {
            CurrentPage = 1;
            await LoadJournaux();
        }

        public async Task ToggleSortOrder()
        {
            // Si c'est 1 (croissant), on passe à 0 (décroissant), sinon on met 1.
            SortOrder = (SortOrder == 1) ? 0 : 1;

            CurrentPage = 1;
            await LoadJournaux();
        }

        public void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                _ = LoadJournaux();
            }
        }

        public void PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                _ = LoadJournaux();
            }
        }
    }
}