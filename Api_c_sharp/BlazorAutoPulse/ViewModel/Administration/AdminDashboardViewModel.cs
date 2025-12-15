using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminDashboardViewModel
    {
        private readonly ICompteService _compteService;
        private readonly ISignalementService _signalementService;

        public int TotalUtilisateurs { get; private set; }
        public int NouveauxUtilisateurs { get; private set; }
        public int TotalAnnonces { get; private set; }
        public int NouvellesAnnonces { get; private set; }
        public int SignalementsEnAttente { get; private set; }
        public decimal RevenuMensuel { get; private set; }
        public decimal CroissanceRevenu { get; private set; }

        // Activités récentes

        public List<AdminSignalement> SignalementsRecent { get; private set; } = new();
        public IEnumerable<SignalementDTO> SignalementsRecentdto { get; private set; }

        private Action? _refreshUI;

        public AdminDashboardViewModel(ICompteService compteService, ISignalementService signalementService)
        {
            _compteService = compteService;
            _signalementService = signalementService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            // Simulation de chargement de données
            await Task.Delay(100);


            TotalUtilisateurs = 1247;
            NouveauxUtilisateurs = 34;
            TotalAnnonces = 523;
            NouvellesAnnonces = 18;
            SignalementsEnAttente = 7;
            RevenuMensuel = 12450;
            CroissanceRevenu = 15.3m;

            SignalementsRecentdto = await _signalementService.GetFiltered(1,0,"");

            if (SignalementsRecentdto == null)
            {
                SignalementsRecent = new List<AdminSignalement>();
            }
            else
            {
                // 3. Mapping
                SignalementsRecent = SignalementsRecentdto.Select(s => new AdminSignalement
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

                _refreshUI?.Invoke();
            }
        }

        public async Task RefreshData()
        {
            await LoadDashboardData();
        }
    }
}