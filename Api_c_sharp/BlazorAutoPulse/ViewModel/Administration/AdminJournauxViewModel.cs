using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminJournauxViewModel
    {
        private readonly IJournalService _journalService;
        private readonly IService<TypeJournalDTO> _typeJournalService;
        private Action? _refreshUI;

        public bool IsLoading { get; private set; } = true;
        public List<JournalDTO> PagedJournaux { get; private set; } = new();
        public List<TypeJournalDTO> TypesJournal { get; private set; } = new();

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 15;
        public int TotalFilteredItems { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalFilteredItems / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        // --- FILTRES ---
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }

        // Filtre Type Journal (nullable pour "Tous" dans l'UI)
        public int? SelectedTypeId { get; set; }

        // Tri : 1 = Croissant, 0 = Décroissant (selon ta logique précédente)
        public int SortOrder { get; private set; } = 0;

        public AdminJournauxViewModel(IJournalService journalService, IService<TypeJournalDTO> typeJournalService)
        {
            _journalService = journalService;
            _typeJournalService = typeJournalService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadTypesJournal();
            await LoadJournaux();
        }

        private async Task LoadTypesJournal()
        {
            try
            {
                var types = await _typeJournalService.GetAllAsync();
                TypesJournal = types.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement types journal: {ex.Message}");
                TypesJournal = new List<TypeJournalDTO>();
            }
        }

        public async Task LoadJournaux()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            // Création du DTO de recherche à partir des propriétés du ViewModel
            var rechercheDto = new RechercheJournalDTO
            {
                IdType = SelectedTypeId ?? 0,
                DebutIntervalle = DateDebut,
                FinIntervalle = DateFin,
                Order = SortOrder
            };

            var result = await _journalService.GetFilteredAsync(rechercheDto);

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