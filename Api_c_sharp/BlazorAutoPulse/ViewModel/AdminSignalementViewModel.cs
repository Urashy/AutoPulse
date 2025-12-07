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
        public string FilterStatus { get; private set; } = "all";

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 9;
        public int TotalPages => FilteredSignalements.Count == 0 ? 1 : (int)Math.Ceiling((double)FilteredSignalements.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public int TotalSignalements => AllSignalements?.Count ?? 0;
        public int SignalementsAnnonces => AllSignalements?.Count(s => s.TypeCible == "Annonce") ?? 0;
        public int SignalementsComptes => AllSignalements?.Count(s => s.TypeCible == "Compte") ?? 0;
        public int SignalementsEnAttente => AllSignalements?.Count(s => s.Statut == "En attente") ?? 0;
        public int SignalementsTraites => AllSignalements?.Count(s => s.Statut == "Traité") ?? 0;
        public int SignalementsRejetes => AllSignalements?.Count(s => s.Statut == "Rejeté") ?? 0;

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
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                await LoadSignalements();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
                AllSignalements = new List<AdminSignalement>();
                FilteredSignalements = new List<AdminSignalement>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        private async Task LoadSignalements()
        {
            try
            {
                Console.WriteLine("Début du chargement des signalements...");

                var signalements = await _signalementService.GetAllAsync();

                Console.WriteLine($"Signalements récupérés: {signalements?.Count() ?? 0}");

                if (signalements == null)
                {
                    Console.WriteLine("Aucun signalement récupéré (null)");
                    AllSignalements = new List<AdminSignalement>();
                    FilteredSignalements = new List<AdminSignalement>();
                    return;
                }

                AllSignalements = signalements.Select(s =>
                {
                    try
                    {
                        return new AdminSignalement
                        {
                            Id = s.IdSignalement,
                            TypeSignalement = s.LibelleTypeSignalement ?? "Type inconnu",
                            // Si IdCompteSignale > 0, c'est un signalement de compte, sinon d'annonce
                            TypeCible = s.IdCompteSignale > 0 ? "Compte" : "Annonce",
                            IdCible = s.IdCompteSignale > 0 ? s.IdCompteSignale : 0,
                            PseudoSignalant = s.PseudoSignalant ?? "Utilisateur inconnu",
                            PseudoCible = s.IdCompteSignale > 0 ? (s.PseudoSignale ?? "Compte inconnu") : null,
                            TitreCible = s.IdCompteSignale == 0 ? "Annonce signalée" : null,
                            Description = s.DescriptionSignalement ?? "",
                            DateSignalement = s.DateCreationSignalement,
                            Statut = "En attente" // Par défaut car IdEtatSignalement n'existe pas dans SignalementDTO
                        };
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur mapping signalement {s.IdSignalement}: {ex.Message}");
                        return null;
                    }
                })
                .Where(s => s != null)
                .Cast<AdminSignalement>()
                .ToList();

                Console.WriteLine($"Signalements mappés: {AllSignalements.Count}");

                ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadSignalements: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                AllSignalements = new List<AdminSignalement>();
                FilteredSignalements = new List<AdminSignalement>();
            }
        }

        public List<AdminSignalement> GetPagedSignalements()
        {
            if (FilteredSignalements == null || FilteredSignalements.Count == 0)
                return new List<AdminSignalement>();

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
            if (AllSignalements == null)
            {
                FilteredSignalements = new List<AdminSignalement>();
                return;
            }

            FilteredSignalements = AllSignalements.ToList();

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
                    (s.TypeSignalement?.ToLower().Contains(query) ?? false) ||
                    (s.PseudoSignalant?.ToLower().Contains(query) ?? false) ||
                    (s.PseudoCible?.ToLower().Contains(query) ?? false) ||
                    (s.TitreCible?.ToLower().Contains(query) ?? false) ||
                    (s.Description?.ToLower().Contains(query) ?? false)
                ).ToList();
            }

            Console.WriteLine($"Après filtres: {FilteredSignalements?.Count ?? 0} signalements");
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
                int nouvelEtat;

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

                    nouvelEtat = 2; // Traité
                    SelectedSignalement.Statut = "Traité";
                }
                else if (ActionType == "reject")
                {
                    nouvelEtat = 3; // Rejeté
                    SelectedSignalement.Statut = "Rejeté";
                }
                else
                {
                    CloseActionModal();
                    return;
                }

                // Mettre à jour le statut du signalement
                await _signalementService.UpdateEtatAsync(SelectedSignalement.Id, nouvelEtat);

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