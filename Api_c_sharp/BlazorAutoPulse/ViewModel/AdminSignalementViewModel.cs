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
        public int SignalementsEnAttente => AllSignalements?.Count(s => s.IdStatut == 1) ?? 0; // MODIFIÉ
        public int SignalementsTraites => AllSignalements?.Count(s => s.IdStatut == 2) ?? 0; // MODIFIÉ
        public int SignalementsRejetes => AllSignalements?.Count(s => s.IdStatut == 3) ?? 0; // MODIFIÉ

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

                // Utilisation de la nouvelle méthode GetAllSignalementsAsync qui retourne SignalementDTO
                var signalements = await _signalementService.GetAllSignalementsAsync();

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

                            TypeCible = s.TypeCible,

                            // IdCible dépend du type
                            IdCible = s.IdAnnonceSignale ?? s.IdCompteSignale ?? 0,

                            // Signalant (toujours présent)
                            PseudoSignalant = s.PseudoSignalant ?? "Utilisateur inconnu",

                            // Cible (selon le type)
                            PseudoCible = s.PseudoSignale,
                            TitreCible = s.LibelleAnnonceSignale,

                            Description = s.DescriptionSignalement ?? "",
                            DateSignalement = s.DateCreationSignalement,


                            // Statut depuis le DTO
                            Statut = s.LibelleEtatSignalement ?? "En attente",

                            IdStatut = s.IdEtatSignalement
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
                .OrderByDescending(s => s.DateSignalement)
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
                FilteredSignalements = FilterStatus switch
                {
                    "pending" => FilteredSignalements.Where(s => s.IdStatut == 1).ToList(), 
                    "resolved" => FilteredSignalements.Where(s => s.IdStatut == 2).ToList(), 
                    "rejected" => FilteredSignalements.Where(s => s.IdStatut == 3).ToList(), 
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
                            Console.WriteLine($"Annonce {SelectedSignalement.IdCible} suspendue");
                        }
                    }
                    else
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
                    SelectedSignalement.IdStatut = 2; 
                }
                else if (ActionType == "reject")
                {
                    nouvelEtat = 3;
                    SelectedSignalement.Statut = "Rejeté";
                    SelectedSignalement.IdStatut = 3; 
                }
                else
                {
                    CloseActionModal();
                    return;
                }

                // Mettre à jour le statut du signalement
                bool success = await _signalementService.UpdateEtatAsync(SelectedSignalement.Id, nouvelEtat);

                if (success)
                {
                    Console.WriteLine($"Signalement {SelectedSignalement.Id} mis à jour avec succès");
                }
                else
                {
                    Console.WriteLine($"Erreur lors de la mise à jour du signalement {SelectedSignalement.Id}");
                }

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

    /// <summary>
    /// Classe représentant un signalement pour l'interface admin
    /// </summary>
    public class AdminSignalement
    {
        public int Id { get; set; }
        public string TypeSignalement { get; set; } = "";
        public string TypeCible { get; set; } = ""; // "Annonce" ou "Compte"
        public int IdCible { get; set; }
        public string PseudoSignalant { get; set; } = "";
        public string? PseudoCible { get; set; } // Pour les signalements de compte
        public string? TitreCible { get; set; } // Pour les signalements d'annonce
        public string? Description { get; set; }
        public DateTime DateSignalement { get; set; }
        public string Statut { get; set; } = ""; // "En attente", "Traité", "Rejeté"
        public int IdStatut { get; set; } // NOUVELLE PROPRIÉTÉ POUR ID
    }
}