using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminDashboardViewModel
    {
        private readonly ICompteService _compteService;
        private readonly ISignalementService _signalementService;
        private readonly IAnnonceService _annonceService;
        private readonly IPlainteService _plainteService;

        public int TotalUtilisateurs { get; private set; }
        public int TotalAnnonces { get; private set; }
        public int SignalementsEnAttente { get; private set; }
        public int TotalPlaintes { get; private set; }

        public List<AdminSignalement> SignalementsRecent { get; private set; } = new();

        private Action? _refreshUI;

        public AdminDashboardViewModel(
            ICompteService compteService,
            ISignalementService signalementService,
            IAnnonceService annonceService,
            IPlainteService plainteService)
        {
            _compteService = compteService;
            _signalementService = signalementService;
            _annonceService = annonceService;
            _plainteService = plainteService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            try
            {
                var comptes = await _compteService.GetAllAsync();
                TotalUtilisateurs = comptes?.Count() ?? 0;

                var annonces = await _annonceService.GetAllAsync();
                TotalAnnonces = annonces?.Count() ?? 0;

                var plaintes = await _plainteService.GetAllAsync();
                TotalPlaintes = plaintes?.Count() ?? 0;

                var signalements = await _signalementService.GetAllSignalementsAsync();
                SignalementsEnAttente = signalements?.Count(s => s.IdEtatSignalement == 1) ?? 0;

                // Activité récente (Signalements)
                var recentSignalementsDto = await _signalementService.GetFiltered(1, 0, "");
                if (recentSignalementsDto != null)
                {
                    SignalementsRecent = recentSignalementsDto.Take(5).Select(s => new AdminSignalement
                    {
                        Id = s.IdSignalement,
                        TypeSignalement = s.LibelleTypeSignalement ?? "Inconnu",
                        PseudoSignalant = s.PseudoSignalant ?? "Anonyme",
                        PseudoCible = s.PseudoSignale,
                        TitreCible = s.LibelleAnnonceSignale,
                        Description = s.DescriptionSignalement ?? "",
                        DateSignalement = s.DateCreationSignalement,
                        Statut = s.LibelleEtatSignalement ?? "En attente"
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur Dashboard : {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        public async Task RefreshData()
        {
            await LoadDashboardData();
        }
    }
}