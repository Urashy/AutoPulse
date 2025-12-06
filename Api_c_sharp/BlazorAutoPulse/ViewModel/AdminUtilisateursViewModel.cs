using BlazorAutoPulse.Model;
using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminUtilisateursViewModel
    {
        private readonly ICompteService _compteService;
        public AdminUtilisateursViewModel(ICompteService compteService)
        {
            _compteService = compteService;
        }
        private List<CompteGetDTO> AllUtilisateurs { get; set; } = new();
        public List<CompteGetDTO> FilteredUtilisateurs { get; private set; } = new();

        public string SearchQuery { get; set; } = "";
        public string FilterStatus { get; private set; } = "all";

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)FilteredUtilisateurs.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        // Propriétés de statistiques (basées sur le libellé du type de compte)
        public int TotalUtilisateurs => AllUtilisateurs.Count;
        public int UtilisateursParticuliers => AllUtilisateurs.Count(u => u.TypeCompte == "Particulier");
        public int UtilisateursPros => AllUtilisateurs.Count(u => u.TypeCompte == "Professionnel");
        public int UtilisateursAdmins => AllUtilisateurs.Count(u => u.TypeCompte == "Administrateur");
        public int UtilisateursAnonymisés => AllUtilisateurs.Count(u => u.TypeCompte == "Anonyme");


        // Propriétés UI
        public bool IsLoading { get; private set; } = true;
        public bool ShowDetailsModal { get; private set; } = false;
        public Compte? SelectedUser { get; private set; }

        private Action? _refreshUI;

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadUsers();
        }

        // Attendre la tâche et transformer en List
        private async Task LoadUsers()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            var utilisateurs = await _compteService.GetAllAsync();
            AllUtilisateurs = utilisateurs.ToList();

            FilteredUtilisateurs = new List<Compte>(AllUtilisateurs);
            IsLoading = false;
            _refreshUI?.Invoke();
        }

        public async Task FilterByStatus(string status)
        {
            FilterStatus = status;
            ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task SearchUsers()
        {
            ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        private void ApplyFilters()
        {
            FilteredUtilisateurs = AllUtilisateurs;

            // Filtre par statut (basé sur le libellé du type de compte)
            if (FilterStatus != "all")
            {
                FilteredUtilisateurs = FilterStatus switch
                {
                    "Particuliers" => FilteredUtilisateurs.Where(u => u.TypeCompte == "Particulier").ToList(),
                    "Pros" => FilteredUtilisateurs.Where(u => u.TypeCompte == "Professionnel").ToList(),
                    "Admin" => FilteredUtilisateurs.Where(u => u.TypeCompte == "Administrateur").ToList(),
                    "Anonymes" => FilteredUtilisateurs.Where(u => u.TypeCompte == "Anonyme").ToList(),
                    _ => FilteredUtilisateurs
                };
            }

            // Filtre par recherche
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                FilteredUtilisateurs = FilteredUtilisateurs.Where(u =>
                    (u.Pseudo != null && u.Pseudo.ToLower().Contains(query)) ||
                    (u.Nom != null && u.Nom.ToLower().Contains(query)) ||
                    (u.Prenom != null && u.Prenom.ToLower().Contains(query))
                ).ToList();
            }
        }

        public void NextPage()
        {
            if (CanGoNext)
            {
                CurrentPage++;
                _refreshUI?.Invoke();
            }
        }

        public void PreviousPage()
        {
            if (CanGoPrevious)
            {
                CurrentPage--;
                _refreshUI?.Invoke();
            }
        }

        public void ViewUserDetails(Compte user)
        {
            SelectedUser = user;
            ShowDetailsModal = true;
            _refreshUI?.Invoke();
        }

        public void CloseDetailsModal()
        {
            ShowDetailsModal = false;
            SelectedUser = null;
            _refreshUI?.Invoke();
        }

        public async Task EditUser(Compte user)
        {
            // Logique d'édition
            Console.WriteLine($"Édition de l'utilisateur: {user.Pseudo}");
            await Task.CompletedTask;
        }

        public async Task SuspendUser(Compte user)
        {
            //a faire
        }

        public async Task ActivateUser(Compte user)
        {
            //a faire
        }

        public async Task DeleteUser(Compte user)
        {
            AllUtilisateurs.Remove(user);
            ApplyFilters();
            Console.WriteLine($"Utilisateur supprimé: {user.Pseudo}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task ExportUsers()
        {
            Console.WriteLine("Export des utilisateurs...");
            await Task.CompletedTask;
        }
    }
}