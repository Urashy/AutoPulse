using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminSignalementsViewModel
    {
        private readonly ISignalementService _signalementService;
        private readonly IAnnonceService _annonceService;
        private readonly ICompteService _compteService;

        // Liste contenant les données brutes chargées depuis l'API (filtrées par TYPE uniquement)
        private List<AdminSignalement> LoadedSignalements { get; set; } = new();

        // Liste affichée dans la grille (filtrée par TYPE + STATUT pour la pagination)
        public List<AdminSignalement> FilteredSignalements { get; private set; } = new();

        public string SearchQuery { get; set; } = "";

        // Filtre de Cible : "all", "annonce", "compte"
        public string FilterType { get; private set; } = "all";

        // Filtre de Statut : 0 = Tous, 1 = En attente, 2 = Traité, 3 = Rejeté
        public int FilterStatus { get; private set; } = 0;

        // --- COMPTEURS FIXES (Onglets du haut) ---
        // Ils sont calculés au chargement initial ("All") et ne changent pas quand on filtre.
        public int FixedTotalAnnonces { get; private set; } = 0;
        public int FixedTotalComptes { get; private set; } = 0;
        public int FixedTotal => FixedTotalAnnonces + FixedTotalComptes;

        // --- COMPTEURS DYNAMIQUES (Onglets du bas) ---
        // Ils dépendent de LoadedSignalements, donc du TYPE sélectionné.
        public int SignalementsEnAttente => LoadedSignalements.Count(s => s.IdStatut == 1);
        public int SignalementsTraites => LoadedSignalements.Count(s => s.IdStatut == 2);
        public int SignalementsRejetes => LoadedSignalements.Count(s => s.IdStatut == 3);

        // Total affiché (dépend du type sélectionné)
        public int TotalSignalements => LoadedSignalements.Count;

        // Pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 9;
        public int TotalPages => FilteredSignalements.Count == 0 ? 1 : (int)Math.Ceiling((double)FilteredSignalements.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public bool IsLoading { get; private set; } = true;

        // --- Modales ---
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
            // Au démarrage, on charge TOUT ("all") pour pouvoir initialiser les compteurs fixes
            FilterType = "all";
            await LoadSignalementsFromApi();
        }

        /// <summary>
        /// Charge les signalements depuis l'API selon le FilterType (Annonce/Compte/All)
        /// Met à jour les compteurs fixes si nécessaire.
        /// </summary>
        private async Task LoadSignalementsFromApi()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                // 1. Conversion du filtre UI vers ID API
                int typeIdApi = FilterType switch
                {
                    "annonce" => 1,
                    "compte" => 2,
                    _ => 0
                };

                // 2. Appel API : On demande le statut 0 (Tous) pour le Type donné
                // Cela nous permet de calculer localement les compteurs En Attente/Traité/Rejeté
                var signalementsDTO = await _signalementService.GetFiltered(0, typeIdApi, SearchQuery);

                if (signalementsDTO == null)
                {
                    LoadedSignalements = new List<AdminSignalement>();
                }
                else
                {
                    // 3. Mapping
                    LoadedSignalements = signalementsDTO.Select(s => new AdminSignalement
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
                    }).ToList();

                    // 4. Mémorisation des totaux fixes (uniquement quand on est sur "Tous" sans recherche)
                    if (FilterType == "all" && string.IsNullOrWhiteSpace(SearchQuery))
                    {
                        FixedTotalAnnonces = LoadedSignalements.Count(s => s.TypeCible == "Annonce");
                        FixedTotalComptes = LoadedSignalements.Count(s => s.TypeCible == "Compte");
                    }
                }

                // 5. Application du filtre de statut local pour l'affichage
                ApplyLocalStatusFilter();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur LoadSignalementsFromApi: {ex.Message}");
                LoadedSignalements = new List<AdminSignalement>();
                FilteredSignalements = new List<AdminSignalement>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        /// <summary>
        /// Applique le filtre de statut (onglets du bas) sur les données déjà chargées en mémoire.
        /// </summary>
        private void ApplyLocalStatusFilter()
        {
            var query = LoadedSignalements.AsEnumerable();

            if (FilterStatus != 0) // 0 = Tous
            {
                query = query.Where(s => s.IdStatut == FilterStatus);
            }

            FilteredSignalements = query.OrderByDescending(s => s.DateSignalement).ToList();
            _refreshUI?.Invoke();
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

        // --- Gestion des Filtres ---

        public async Task FilterByType(string type)
        {
            if (FilterType != type)
            {
                FilterType = type;
                CurrentPage = 1;
                await LoadSignalementsFromApi(); // Appel API nécessaire car on change de scope de données
            }
        }

        public async Task FilterByStatus(int statusId)
        {
            if (FilterStatus != statusId)
            {
                FilterStatus = statusId;
                CurrentPage = 1;
                ApplyLocalStatusFilter(); // Filtre local suffisant
                await Task.CompletedTask;
            }
        }

        public async Task SearchSignalements()
        {
            CurrentPage = 1;
            await LoadSignalementsFromApi(); // La recherche passe par l'API
        }

        // --- Pagination ---
        public void NextPage() { if (CanGoNext) { CurrentPage++; _refreshUI?.Invoke(); } }
        public void PreviousPage() { if (CanGoPrevious) { CurrentPage--; _refreshUI?.Invoke(); } }

        // --- Modales ---
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
                ActionMessage = "Action sur l'annonce ?";
                SelectedAction = "delete";
            }
            else
            {
                ActionMessage = "Action sur le compte ?";
                SelectedAction = "anonymize";
            }
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

        public void CloseActionModal()
        {
            ShowActionModal = false;
            SelectedSignalement = null;
            _refreshUI?.Invoke();
        }

        public async Task ConfirmAction()
        {
            if (SelectedSignalement == null) return;
            try
            {
                int nouvelEtat = SelectedSignalement.IdStatut; // Par défaut

                // 1. Logique métier spécifique
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
                }
                else if (ActionType == "reject")
                {
                    nouvelEtat = 3; // Rejeté
                }
                else
                {
                    CloseActionModal();
                    return;
                }

                // 2. Mise à jour du signalement via l'API
                var signalementComplet = await _signalementService.GetByIdAsync(SelectedSignalement.Id);

                if (signalementComplet != null)
                {
                    var updateDto = new SignalementUpdateDTO
                    {
                        IdSignalement = SelectedSignalement.Id,
                        DescriptionSignalement = signalementComplet.DescriptionSignalement,
                        IdCompteSignalant = signalementComplet.IdCompteSignalant,
                        IdAnnonceSignale = signalementComplet.IdAnnonceSignale,
                        IdCompteSignale = signalementComplet.IdCompteSignale,
                        IdTypeSignalement = signalementComplet.IdTypeSignalement,
                        IdEtatSignalement = nouvelEtat
                    };

                    await _signalementService.UpdateSignalementAsync(SelectedSignalement.Id, updateDto);
                }

                CloseActionModal();

                // 3. Rechargement des données
                await LoadSignalementsFromApi();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur ConfirmAction: {ex.Message}");
            }
        }
    }
}