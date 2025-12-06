using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminUtilisateursViewModel
    {
        private readonly ICompteService _compteService;
        private Action? _refreshUI;

        // Liste complète des utilisateurs
        public List<CompteDetailDTO> AllUtilisateurs { get; set; } = new();

        // Liste filtrée
        public List<CompteDetailDTO> FilteredUtilisateurs { get; set; } = new();

        // État de chargement
        public bool IsLoading { get; set; } = true;

        // Recherche
        public string SearchQuery { get; set; } = "";

        // Filtres
        public string FilterStatus { get; set; } = "all";

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling((double)FilteredUtilisateurs.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        // Statistiques
        public int TotalUtilisateurs => AllUtilisateurs.Count;
        public int UtilisateursParticuliers => AllUtilisateurs.Count(u => u.TypeCompte == "Particulier");
        public int UtilisateursPros => AllUtilisateurs.Count(u => u.TypeCompte == "Professionnel");
        public int UtilisateursAdmins => AllUtilisateurs.Count(u => u.TypeCompte == "Administrateur");
        public int UtilisateursAnonymisés => AllUtilisateurs.Count(u => u.TypeCompte == "Anonyme");

        // Modal de détails
        public bool ShowDetailsModal { get; set; }
        public CompteDetailDTO? SelectedUser { get; set; }

        public AdminUtilisateursViewModel(ICompteService compteService)
        {
            _compteService = compteService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadUsers();
        }

        public async Task LoadUsers()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                Console.WriteLine("🔄 Début du chargement des utilisateurs...");

                // Récupérer tous les comptes
                var response = await _compteService.GetAllAsync();

                Console.WriteLine($"✅ Réponse reçue: {response}");

                if (response == null)
                {
                    Console.WriteLine("⚠️ La réponse est null !");
                    AllUtilisateurs = new List<CompteDetailDTO>();
                }
                else
                {
                    // Vérifier le type de réponse
                    Console.WriteLine($"📦 Type de réponse: {response.GetType().Name}");

                    // Selon ton CompteWebService, GetAllAsync retourne CompteGetDTO
                    // Donc on va essayer de caster
                    if (response is IEnumerable<CompteGetDTO> comptesGet)
                    {
                        Console.WriteLine($"📊 Nombre de comptes reçus: {comptesGet.Count()}");

                        // Conversion de CompteGetDTO vers CompteDetailDTO
                        AllUtilisateurs = comptesGet.Select(c => new CompteDetailDTO
                        {
                            IdCompte = c.IdCompte,
                            Pseudo = c.Pseudo,
                            Nom = c.Nom,
                            Prenom = c.Prenom,
                            Email = "", // À ajouter dans CompteGetDTO
                            TypeCompte = c.TypeCompte,
                            DateCreation = c.DateInscription,
                            IdTypeCompte = 0 // Tu peux mapper ça si nécessaire
                        }).ToList();

                        Console.WriteLine($"✅ {AllUtilisateurs.Count} utilisateurs chargés");
                    }
                    else if (response is IEnumerable<CompteDetailDTO> comptesDetail)
                    {
                        AllUtilisateurs = comptesDetail.ToList();
                        Console.WriteLine($"✅ {AllUtilisateurs.Count} utilisateurs détaillés chargés");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Type de réponse non géré: {response.GetType().FullName}");
                        AllUtilisateurs = new List<CompteDetailDTO>();
                    }
                }

                ApplyFilters();

                Console.WriteLine($"🎯 Filtrage appliqué. {FilteredUtilisateurs.Count} utilisateurs après filtres");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR lors du chargement: {ex.Message}");
                Console.WriteLine($"📍 StackTrace: {ex.StackTrace}");
                AllUtilisateurs = new List<CompteDetailDTO>();
                FilteredUtilisateurs = new List<CompteDetailDTO>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public void SearchUsers()
        {
            ApplyFilters();
        }

        public void FilterByStatus(string status)
        {
            FilterStatus = status;
            CurrentPage = 1; // Reset à la première page
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            // On commence avec tous les utilisateurs
            var filtered = AllUtilisateurs.AsEnumerable();

            // Filtre par type de compte
            if (FilterStatus != "all")
            {
                filtered = filtered.Where(u => u.TypeCompte.Equals(FilterStatus, StringComparison.OrdinalIgnoreCase));
            }

            // Filtre par recherche
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(u =>
                    u.Pseudo.ToLower().Contains(query) ||
                    u.Nom.ToLower().Contains(query) ||
                    u.Prenom.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query));
            }

            // Conversion finale en List
            FilteredUtilisateurs = filtered.ToList();

            _refreshUI?.Invoke();
        }

        // Pagination
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

        // Actions utilisateur
        public void ViewUserDetails(CompteDetailDTO user)
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

        public async Task EditUser(CompteDetailDTO user)
        {
            // TODO: Implémenter l'édition
            Console.WriteLine($"Édition de l'utilisateur {user.Pseudo}");
        }

        public async Task SuspendUser(CompteDetailDTO user)
        {
            // TODO: Implémenter la suspension
            Console.WriteLine($"Suspension de l'utilisateur {user.Pseudo}");
        }

        public async Task ActivateUser(CompteDetailDTO user)
        {
            // TODO: Implémenter l'activation
            Console.WriteLine($"Activation de l'utilisateur {user.Pseudo}");
        }

        public async Task DeleteUser(CompteDetailDTO user)
        {
            if (await ConfirmDelete(user))
            {
                try
                {
                    await _compteService.DeleteAsync(user.IdCompte);
                    await LoadUsers();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la suppression: {ex.Message}");
                }
            }
        }

        private async Task<bool> ConfirmDelete(CompteDetailDTO user)
        {
            // TODO: Afficher une modale de confirmation
            return true; // Pour l'instant
        }

        public async Task ExportUsers()
        {
            // TODO: Implémenter l'export CSV/Excel
            Console.WriteLine("Export des utilisateurs");
        }
    }
}