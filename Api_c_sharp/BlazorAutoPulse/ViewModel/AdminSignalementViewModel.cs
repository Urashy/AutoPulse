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

        // On garde string ici car l'UI filtre par CIBLE (Annonce vs Compte) et non par MOTIF (1-10)
        public string FilterType { get; private set; } = "all";

        // CORRECTION : Passage en int pour correspondre aux IDs de la BDD
        // 0 = Tous, 1 = En attente, 2 = Résolu, 3 = Rejeté
        public int FilterStatus { get; private set; } = 0;

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 9;
        public int TotalPages => FilteredSignalements.Count == 0 ? 1 : (int)Math.Ceiling((double)FilteredSignalements.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public int TotalSignalements => AllSignalements?.Count ?? 0;
        public int SignalementsAnnonces => AllSignalements?.Count(s => s.TypeCible == "Annonce") ?? 0;
        public int SignalementsComptes => AllSignalements?.Count(s => s.TypeCible == "Compte") ?? 0;

        // Utilisation des IDs constants
        public int SignalementsEnAttente => AllSignalements?.Count(s => s.IdStatut == 1) ?? 0;
        public int SignalementsTraites => AllSignalements?.Count(s => s.IdStatut == 2) ?? 0;
        public int SignalementsRejetes => AllSignalements?.Count(s => s.IdStatut == 3) ?? 0;

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
                var signalements = await _signalementService.GetAllSignalementsAsync();

                if (signalements == null)
                {
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
                            IdCible = s.IdAnnonceSignale ?? s.IdCompteSignale ?? 0,
                            PseudoSignalant = s.PseudoSignalant ?? "Utilisateur inconnu",
                            PseudoCible = s.PseudoSignale,
                            TitreCible = s.LibelleAnnonceSignale,
                            Description = s.DescriptionSignalement ?? "",
                            DateSignalement = s.DateCreationSignalement,
                            Statut = s.LibelleEtatSignalement ?? "En attente",
                            IdStatut = s.IdEtatSignalement
                        };
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(s => s != null)
                .Cast<AdminSignalement>()
                .OrderByDescending(s => s.DateSignalement)
                .ToList();

                await ApplyFilters();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadSignalements: {ex.Message}");
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
            await ApplyFilters();
            _refreshUI?.Invoke();
        }

        // CORRECTION : Accepte maintenant un int (0, 1, 2, 3)
        public async Task FilterByStatus(int statusId)
        {
            FilterStatus = statusId;
            CurrentPage = 1;
            await ApplyFilters();
            _refreshUI?.Invoke();
        }

        public async Task SearchSignalements()
        {
            await ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
        }

        private async Task ApplyFilters()
        {
            if (AllSignalements == null)
            {
                FilteredSignalements = new List<AdminSignalement>();
                return;
            }

            // Filtrage initial en mémoire (ou appel service si optimisé)
            var query = AllSignalements.AsEnumerable();

            // Filtre par statut (ID)
            if (FilterStatus != 0) // 0 = All
            {
                query = query.Where(s => s.IdStatut == FilterStatus);
            }

            // Filtre par type cible (String : Annonce/Compte)
            if (FilterType != "all")
            {
                // Note : Assure-toi que TypeCible dans la BDD correspond bien à "Annonce" ou "Compte"
                // Sinon utilise StringComparison.OrdinalIgnoreCase
                query = query.Where(s => s.TypeCible?.ToLower() == FilterType.ToLower());
            }

            // Filtre recherche
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                // Si tu veux continuer à utiliser le service filtré backend :
                // await _signalementService.GetFiltered(FilterStatus, FilterType, SearchQuery);
                // Mais attention, il faut mettre à jour la méthode du service pour accepter un INT pour le statut.

                // Filtrage mémoire simple pour l'exemple :
                query = query.Where(s =>
                   (s.PseudoSignalant?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (s.PseudoCible?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (s.TitreCible?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false)
                );
            }

            FilteredSignalements = query.ToList();
            _refreshUI?.Invoke();
        }

        public void NextPage() { if (CanGoNext) { CurrentPage++; _refreshUI?.Invoke(); } }
        public void PreviousPage() { if (CanGoPrevious) { CurrentPage--; _refreshUI?.Invoke(); } }

        public void ViewDetails(AdminSignalement signalement) { SelectedSignalement = signalement; ShowDetailsModal = true; _refreshUI?.Invoke(); }
        public void CloseDetailsModal() { ShowDetailsModal = false; SelectedSignalement = null; _refreshUI?.Invoke(); }

        public void AcceptSignalement(AdminSignalement signalement)
        {
            SelectedSignalement = signalement;
            ActionType = "accept";
            if (signalement.TypeCible == "Annonce") { ActionMessage = "Action sur l'annonce ?"; SelectedAction = "delete"; }
            else { ActionMessage = "Action sur le compte ?"; SelectedAction = "anonymize"; }
            ShowActionModal = true;
            _refreshUI?.Invoke();
        }

        public void RejectSignalement(AdminSignalement signalement)
        {
            SelectedSignalement = signalement;
            ActionType = "reject";
            ActionMessage = $"Rejeter ce signalement ?";
            ShowActionModal = true;
            _refreshUI?.Invoke();
        }

        public void SetAction(string action) { SelectedAction = action; _refreshUI?.Invoke(); }
        public void CloseActionModal() { ShowActionModal = false; SelectedSignalement = null; _refreshUI?.Invoke(); }

        public async Task ConfirmAction()
        {
            if (SelectedSignalement == null) return;
            try
            {
                int nouvelEtat = SelectedSignalement.IdStatut; // Par défaut

                // 1. Logique pour déterminer le nouvel état et les actions annexes
                if (ActionType == "accept")
                {
                    if (SelectedSignalement.TypeCible == "Annonce")
                    {
                        if (SelectedAction == "delete") await _annonceService.DeleteAsync(SelectedSignalement.IdCible);
                    }
                    else // Compte
                    {
                        if (SelectedAction == "anonymize") await _compteService.Anonymisation(SelectedSignalement.IdCible);
                        else if (SelectedAction == "suspend") await _compteService.ToggleSuspention(SelectedSignalement.IdCible, false);
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

                // --- CORRECTION MAJEURE ICI ---

                // 2. Récupérer l'objet complet depuis l'API pour ne pas perdre de données
                var signalementComplet = await _signalementService.GetByIdAsync(SelectedSignalement.Id);

                if (signalementComplet != null)
                {
                    // 3. Mapper vers le UpdateDTO
                    var updateDto = new SignalementUpdateDTO
                    {
                        IdSignalement = SelectedSignalement.Id,
                        DescriptionSignalement = signalementComplet.DescriptionSignalement,
                        IdCompteSignalant = signalementComplet.IdCompteSignalant,
                        IdAnnonceSignale = signalementComplet.IdAnnonceSignale,
                        IdCompteSignale = signalementComplet.IdCompteSignale,
                        IdTypeSignalement = signalementComplet.IdTypeSignalement,

                        // 4. Appliquer le nouvel état
                        IdEtatSignalement = nouvelEtat
                    };

                    // 5. Appeler le service corrigé
                    await _signalementService.UpdateSignalementAsync(SelectedSignalement.Id, updateDto);

                    // Mettre à jour l'affichage local
                    SelectedSignalement.IdStatut = nouvelEtat;
                }

                CloseActionModal();
                await ApplyFilters();
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
    }
}