using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminUtilisateursViewModel
    {
        private readonly ICompteService _compteService;
        private Action? _refreshUI;

        public List<CompteDetailDTO> AllUtilisateurs { get; set; } = new();

        public List<CompteDetailDTO> FilteredUtilisateurs { get; set; } = new();

        public bool IsLoading { get; set; } = true;

        public string SearchQuery { get; set; } = "";

        public string FilterStatus { get; set; } = "all";

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

                var response = await _compteService.GetAllAsync();

                if (response == null)
                {
                    Console.WriteLine("⚠️ La réponse est null !");
                    AllUtilisateurs = new List<CompteDetailDTO>();
                    FilteredUtilisateurs = new List<CompteDetailDTO>(); // ✅ Ajouter
                }
                else
                {
                    if (response is IEnumerable<CompteGetDTO> comptesGet)
                    {
                        Console.WriteLine($"📊 Nombre de comptes reçus: {comptesGet.Count()}");

                        AllUtilisateurs = comptesGet.Select(c => new CompteDetailDTO
                        {
                            IdCompte = c.IdCompte,
                            Pseudo = c.Pseudo,
                            Nom = c.Nom,
                            Prenom = c.Prenom,
                            Email = "",
                            TypeCompte = c.TypeCompte,
                            DateCreation = c.DateInscription,
                            IdTypeCompte = c.IdTypeCompte
                        }).ToList();

                        Console.WriteLine($"✅ {AllUtilisateurs.Count} utilisateurs chargés");

                        FilteredUtilisateurs = new List<CompteDetailDTO>(AllUtilisateurs);
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR lors du chargement: {ex.Message}");
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

        public async Task FilterByStatus(string status)
        {
            FilterStatus = status;
            CurrentPage = 1;
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                Console.WriteLine($"🔍 Filtrage par statut: {status}");

                if (!AllUtilisateurs.Any())
                {
                    var allResponse = await _compteService.GetAllAsync();
                    AllUtilisateurs = ConvertToDetailDTO(allResponse);
                }

                // ✅ Maintenant charger les données filtrées
                IEnumerable<CompteGetDTO> response;

                if (status == "all")
                {
                    // Si "all", FilteredUtilisateurs = AllUtilisateurs
                    FilteredUtilisateurs = new List<CompteDetailDTO>(AllUtilisateurs);
                }
                else
                {
                    int idTypeCompte = status switch
                    {
                        "Particuliers" => 1,
                        "Pros" => 2,
                        "Admin" => 3,
                        "Anonymes" => 4,
                        _ => 0
                    };

                    if (idTypeCompte == 0)
                    {
                        FilteredUtilisateurs = new List<CompteDetailDTO>(AllUtilisateurs);
                    }
                    else
                    {
                        response = await _compteService.GetByTypeCompteAsync(idTypeCompte);
                        FilteredUtilisateurs = ConvertToDetailDTO(response);
                    }
                }

                Console.WriteLine($"✅ {FilteredUtilisateurs.Count} filtrés / {AllUtilisateurs.Count} total");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR lors du filtrage: {ex.Message}");
                FilteredUtilisateurs = new List<CompteDetailDTO>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        private List<CompteDetailDTO> ConvertToDetailDTO(IEnumerable<CompteGetDTO> comptes)
        {
            return comptes.Select(c => new CompteDetailDTO
            {
                IdCompte = c.IdCompte,
                Pseudo = c.Pseudo,
                Nom = c.Nom,
                Prenom = c.Prenom,
                Email = "",
                TypeCompte = c.TypeCompte,
                DateCreation = c.DateInscription,
                IdTypeCompte = c.IdTypeCompte
            }).ToList();
        }

        private void ApplyFilters()
        {
            var filtered = FilteredUtilisateurs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filtered = filtered.Where(u =>
                    u.Pseudo.ToLower().Contains(query) ||
                    u.Nom.ToLower().Contains(query) ||
                    u.Prenom.ToLower().Contains(query) ||
                    u.Email.ToLower().Contains(query));
            }

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