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

            try
            {
                // Création du DTO de recherche
                var rechercheDto = new RechercheJournalDTO
                {
                    IdType = SelectedTypeId ?? 0,
                    DebutIntervalle = DateDebut,

                    // CORRECTIF DATE : On ajoute 1 jour à la date de fin. 
                    // L'API reçoit minuit (00:00:00). Sans cela, tous les journaux 
                    // créés durant la journée de fin sont exclus par le " <= ".
                    FinIntervalle = DateFin?.AddDays(1),

                    Order = SortOrder
                };

                // Appel au service (qui peut lever une exception si 404)
                var result = await _journalService.GetFilteredAsync(rechercheDto);

                var allFiltered = result.ToList();
                TotalFilteredItems = allFiltered.Count;

                PagedJournaux = allFiltered
                    .Skip((CurrentPage - 1) * ItemsPerPage)
                    .Take(ItemsPerPage)
                    .ToList();
            }
            catch (HttpRequestException ex) when (ex.Message.Contains("404"))
            {
                // CORRECTIF 404 : Si l'API renvoie 404 (aucun résultat),
                // on réinitialise simplement la liste au lieu de faire planter Blazor.
                PagedJournaux = new List<JournalDTO>();
                TotalFilteredItems = 0;
                Console.WriteLine("Info: Aucun journal trouvé pour ces critères (404).");
            }
            catch (Exception ex)
            {
                // Gestion des autres types d'erreurs (réseau, serveur 500, etc.)
                Console.WriteLine($"Erreur lors du chargement: {ex.Message}");
                PagedJournaux = new List<JournalDTO>();
                TotalFilteredItems = 0;
            }
            finally
            {
                // Garanti que le chargement s'arrête, même en cas d'erreur
                IsLoading = false;
                _refreshUI?.Invoke();
            }
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