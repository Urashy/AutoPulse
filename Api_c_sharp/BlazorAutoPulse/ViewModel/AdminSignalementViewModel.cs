using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminSignalementsViewModel
    {
        private readonly ISignalementService _signalementService;
        private readonly IAnnonceService _annonceService;
        private readonly ICompteService _compteService;

        private List<AdminSignalement> AllSignalements { get; set; } = new();
        public List<AdminSignalement> FilteredSignalements { get; private set; } = new();

        public string SearchQuery { get; set; } = "";
        public string FilterType { get; private set; } = "all";
        public string FilterStatus { get; private set; } = "pending";

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 9;
        public int TotalPages => (int)Math.Ceiling((double)FilteredSignalements.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public int TotalSignalements => AllSignalements.Count;
        public int SignalementsAnnonces => AllSignalements.Count(s => s.TypeCible == "Annonce");
        public int SignalementsComptes => AllSignalements.Count(s => s.TypeCible == "Compte");
        public int SignalementsEnAttente => AllSignalements.Count(s => s.Statut == "En attente");
        public int SignalementsTraites => AllSignalements.Count(s => s.Statut == "Traité");
        public int SignalementsRejetes => AllSignalements.Count(s => s.Statut == "Rejeté");

        public bool IsLoading { get; private set; } = true;

        // Modal de confirmation
        public bool ShowActionModal { get; private set; }
        public bool ShowDetailsModal { get; private set; }
        public AdminSignalement? SelectedSignalement { get; private set; }
        public string ActionType { get; private set; } = "";
        public string SelectedAction { get; private set; } = "";
        public string ActionMessage { get; private set; } = "";

        private Action? _refreshUI;

        public AdminSignalementsViewModel(
            ISignalementService signalementService,
            IAnnonceService annonceService,
            ICompteService compteService)
        {
            _signalementService = signalementService;
            _annonceService = annonceService;
            _compteService = compteService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadSignalements();
        }

        private async Task LoadSignalements()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                var signalements = await _signalementService.GetAllAsync();

                AllSignalements = signalements.Select(s => new AdminSignalement
                {
                    Id = s.IdSignalement,
                    TypeSignalement = s.LibelleTypeSignalement,
                    TypeCible = s.IdCompteSignale.HasValue ? "Compte" : "Annonce",
                    IdCible = s.IdCompteSignale ?? s.IdAnnonceSignale ?? 0,
                    PseudoSignalant = s.PseudoSignalant,
                    PseudoCible = s.IdCompteSignale.HasValue ? GetPseudoCompte(s.IdCompteSignale.Value) : null,
                    TitreCible = s.IdAnnonceSignale ? s.LibelleAnnonceSignale : null,
                    Description = s.DescriptionSignalement,
                    DateSignalement = s.DateCreationSignalement,
                    Statut = GetStatutText(s.IdEtatSignalement)
                }).ToList();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement signalements: {ex.Message}");
                AllSignalements = new List<AdminSignalement>();
                FilteredSignalements = new List<AdminSignalement>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        private string GetStatutText(int idEtat)
        {
            return idEtat switch
            {
                1 => "En attente",
                2 => "Traité",
                3 => "Rejeté",
                _ => "Inconnu"
            };
        }

        private string GetPseudoCompte(int idCompte)
        {
            // TODO: Récupérer le vrai pseudo via le service
            return $"Utilisateur#{idCompte}";
        }

        public List<AdminSignalement> GetPagedSignalements()
        {
            return FilteredSignalements
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();
        }

        public async Task FilterByType(string type)
        {
            FilterType = type;
            CurrentPage = 1;
            ApplyFilters();
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task FilterByStatus(string status)
        {
            FilterStatus = status;
            CurrentPage = 1;
            ApplyFilters();
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task SearchSignalements()
        {
            ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        private void ApplyFilters()
        {
            FilteredSignalements = AllSignalements;

            // Filtre par type
            if (FilterType != "all")
            {
                FilteredSignalements = FilterType switch
                {
                    "annonce" => FilteredSignalements.Where(s => s.TypeCible == "Annonce").ToList(),
                    "compte" => FilteredSignalements.Where(s => s.TypeCible == "Compte").ToList(),
                    _ => FilteredSignalements
                };
            }

            // Filtre par statut
            if (FilterStatus != "all")
            {
                FilteredSignalements = FilterStatus switch
                {
                    "pending" => FilteredSignalements.Where(s => s.Statut == "En attente").ToList(),
                    "resolved" => FilteredSignalements.Where(s => s.Statut == "Traité").ToList(),
                    "rejected" => FilteredSignalements.Where(s => s.Statut == "Rejeté").ToList(),
                    _ => FilteredSignalements
                };
            }

            // Recherche
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                FilteredSignalements = FilteredSignalements.Where(s =>
                    s.TypeSignalement.ToLower().Contains(query) ||
                    s.PseudoSignalant.ToLower().Contains(query) ||
                    (s.PseudoCible?.ToLower().Contains(query) ?? false) ||
                    (s.TitreCible?.ToLower().Contains(query) ?? false) ||
                    (s.Description?.ToLower().Contains(query) ?? false)
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

        public void ViewDetails(AdminSignalement signalement)
        {
            SelectedSignalement = signalement;
            ShowDetailsModal = true;
            _refreshUI?.Invoke();
        }

        public void CloseDetailsModal()
        {
            ShowDetailsModal = false;
            SelectedSignalement = null;
            _refreshUI?.Invoke();
        }

        public void AcceptSignalement(AdminSignalement signalement)
        {
            SelectedSignalement = signalement;
            ActionType = "accept";

            if (signalement.TypeCible == "Annonce")
            {
                ActionMessage = "Quelle action souhaitez-vous effectuer sur cette annonce ?";
                SelectedAction = "delete";
            }
            else
            {
                ActionMessage = "Quelle action souhaitez-vous effectuer sur ce compte ?";
                SelectedAction = "anonymize";
            }

            ShowActionModal = true;
            _refreshUI?.Invoke();
        }

        public void RejectSignalement(AdminSignalement signalement)
        {
            SelectedSignalement = signalement;
            ActionType = "reject";
            ActionMessage = $"Êtes-vous sûr de vouloir rejeter ce signalement ?";
            ShowActionModal = true;
            _refreshUI?.Invoke();
        }

        public void SetAction(string action)
        {
            SelectedAction = action;
            _refreshUI?.Invoke();
        }

        public void CloseActionModal()
        {
            ShowActionModal = false;
            SelectedSignalement = null;
            ActionType = "";
            SelectedAction = "";
            ActionMessage = "";
            _refreshUI?.Invoke();
        }

        public async Task ConfirmAction()
        {
            if (SelectedSignalement == null) return;

            try
            {
                if (ActionType == "accept")
                {
                    if (SelectedSignalement.TypeCible == "Annonce")
                    {
                        if (SelectedAction == "delete")
                        {
                            await _annonceService.DeleteAsync(SelectedSignalement.IdCible);
                            Console.WriteLine($"Annonce {SelectedSignalement.IdCible} supprimée");
                        }
                        else if (SelectedAction == "suspend")
                        {
                            // TODO: Implémenter la suspension d'annonce
                            Console.WriteLine($"Annonce {SelectedSignalement.IdCible} suspendue");
                        }
                    }
                    else // Compte
                    {
                        if (SelectedAction == "anonymize")
                        {
                            await _compteService.Anonymisation(SelectedSignalement.IdCible);
                            Console.WriteLine($"Compte {SelectedSignalement.IdCible} anonymisé");
                        }
                        else if (SelectedAction == "suspend")
                        {
                            // TODO: Implémenter la suspension de compte
                            Console.WriteLine($"Compte {SelectedSignalement.IdCible} suspendu");
                        }
                    }

                    SelectedSignalement.Statut = "Traité";
                }
                else if (ActionType == "reject")
                {
                    SelectedSignalement.Statut = "Rejeté";
                }

                // TODO: Mettre à jour le statut du signalement dans la base de données
                // await _signalementService.UpdateEtatAsync(SelectedSignalement.Id, newEtat);

                CloseActionModal();
                ApplyFilters();
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la confirmation de l'action: {ex.Message}");
            }
        }
    }

    public class AdminSignalement
    {
        public int Id { get; set; }
        public string TypeSignalement { get; set; } = "";
        public string TypeCible { get; set; } = "";
        public int IdCible { get; set; }
        public string PseudoSignalant { get; set; } = "";
        public string? PseudoCible { get; set; }
        public string? TitreCible { get; set; }
        public string? Description { get; set; }
        public DateTime DateSignalement { get; set; }
        public string Statut { get; set; } = "";
    }
}