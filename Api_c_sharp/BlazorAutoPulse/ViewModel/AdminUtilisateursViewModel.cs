namespace BlazorAutoPulse.ViewModel
{
    public class AdminUtilisateursViewModel
    {
        // Propriétés de données
        private List<AdminUtilisateur> AllUtilisateurs { get; set; } = new();
        public List<AdminUtilisateur> FilteredUtilisateurs { get; private set; } = new();

        // Propriétés de filtrage et recherche
        public string SearchQuery { get; set; } = "";
        public string FilterStatus { get; private set; } = "all";

        // Propriétés de pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 20;
        public int TotalPages => (int)Math.Ceiling((double)FilteredUtilisateurs.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        // Propriétés de statistiques
        public int TotalUtilisateurs => AllUtilisateurs.Count;
        public int UtilisateursActifs => AllUtilisateurs.Count(u => u.Statut == "Actif");
        public int UtilisateursSuspendus => AllUtilisateurs.Count(u => u.Statut == "Suspendu");
        public int UtilisateursPro => AllUtilisateurs.Count(u => u.TypeCompte == "Professionnel");

        // Propriétés UI
        public bool IsLoading { get; private set; } = true;
        public bool ShowDetailsModal { get; private set; } = false;
        public AdminUtilisateur? SelectedUser { get; private set; }

        private Action? _refreshUI;

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadUsers();
        }

        private async Task LoadUsers()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            // Simulation de chargement
            await Task.Delay(500);

            // Données simulées
            AllUtilisateurs = new List<AdminUtilisateur>
            {
                new AdminUtilisateur
                {
                    Id = 1,
                    Pseudo = "jdupont",
                    NomComplet = "Jean Dupont",
                    Email = "jean.dupont@email.com",
                    TypeCompte = "Particulier",
                    DateInscription = DateTime.Now.AddMonths(-6),
                    NombreAnnonces = 3,
                    Statut = "Actif",
                    DerniereConnexion = "Il y a 2 heures"
                },
                new AdminUtilisateur
                {
                    Id = 2,
                    Pseudo = "mmartin_pro",
                    NomComplet = "Marie Martin",
                    Email = "marie.martin@concession.fr",
                    TypeCompte = "Professionnel",
                    DateInscription = DateTime.Now.AddYears(-1),
                    NombreAnnonces = 28,
                    Statut = "Actif",
                    DerniereConnexion = "Il y a 10 minutes"
                },
                new AdminUtilisateur
                {
                    Id = 3,
                    Pseudo = "pdurand",
                    NomComplet = "Pierre Durand",
                    Email = "pierre.durand@email.com",
                    TypeCompte = "Particulier",
                    DateInscription = DateTime.Now.AddMonths(-3),
                    NombreAnnonces = 1,
                    Statut = "Suspendu",
                    DerniereConnexion = "Il y a 5 jours"
                },
                new AdminUtilisateur
                {
                    Id = 4,
                    Pseudo = "sgarage_auto",
                    NomComplet = "Sophie Garage",
                    Email = "contact@sophiegarage.fr",
                    TypeCompte = "Professionnel",
                    DateInscription = DateTime.Now.AddMonths(-8),
                    NombreAnnonces = 45,
                    Statut = "Actif",
                    DerniereConnexion = "Il y a 30 minutes"
                },
                new AdminUtilisateur
                {
                    Id = 5,
                    Pseudo = "lbernard",
                    NomComplet = "Luc Bernard",
                    Email = "luc.bernard@email.com",
                    TypeCompte = "Particulier",
                    DateInscription = DateTime.Now.AddMonths(-2),
                    NombreAnnonces = 2,
                    Statut = "Actif",
                    DerniereConnexion = "Il y a 1 jour"
                }
            };

            FilteredUtilisateurs = new List<AdminUtilisateur>(AllUtilisateurs);
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

            // Filtre par statut
            if (FilterStatus != "all")
            {
                FilteredUtilisateurs = FilterStatus switch
                {
                    "active" => FilteredUtilisateurs.Where(u => u.Statut == "Actif").ToList(),
                    "suspended" => FilteredUtilisateurs.Where(u => u.Statut == "Suspendu").ToList(),
                    "pro" => FilteredUtilisateurs.Where(u => u.TypeCompte == "Professionnel").ToList(),
                    _ => FilteredUtilisateurs
                };
            }

            // Filtre par recherche
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                FilteredUtilisateurs = FilteredUtilisateurs.Where(u =>
                    u.Pseudo.ToLower().Contains(query) ||
                    u.NomComplet.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query)
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

        public void ViewUserDetails(AdminUtilisateur user)
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

        public async Task EditUser(AdminUtilisateur user)
        {
            // Logique d'édition
            Console.WriteLine($"Édition de l'utilisateur: {user.Pseudo}");
            await Task.CompletedTask;
        }

        public async Task SuspendUser(AdminUtilisateur user)
        {
            user.Statut = "Suspendu";
            Console.WriteLine($"Utilisateur suspendu: {user.Pseudo}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task ActivateUser(AdminUtilisateur user)
        {
            user.Statut = "Actif";
            Console.WriteLine($"Utilisateur activé: {user.Pseudo}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task DeleteUser(AdminUtilisateur user)
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

    public class AdminUtilisateur
    {
        public int Id { get; set; }
        public string Pseudo { get; set; } = "";
        public string NomComplet { get; set; } = "";
        public string Email { get; set; } = "";
        public string TypeCompte { get; set; } = "";
        public DateTime DateInscription { get; set; }
        public int NombreAnnonces { get; set; }
        public string Statut { get; set; } = "";
        public string DerniereConnexion { get; set; } = "";
        public bool IsSelected { get; set; } = false;

        public string Initiales =>
            string.Join("", NomComplet.Split(' ')
                .Select(n => n.FirstOrDefault())
                .Take(2))
            .ToUpper();
    }
}