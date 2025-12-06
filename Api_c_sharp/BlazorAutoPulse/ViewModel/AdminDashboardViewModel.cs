using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminDashboardViewModel
    {
        private readonly ICompteService _compteService;

        public int TotalUtilisateurs { get; private set; }
        public int NouveauxUtilisateurs { get; private set; }
        public int TotalAnnonces { get; private set; }
        public int NouvellesAnnonces { get; private set; }
        public int SignalementsEnAttente { get; private set; }
        public decimal RevenuMensuel { get; private set; }
        public decimal CroissanceRevenu { get; private set; }

        // Activités récentes
        public List<AdminActivity> RecentActivities { get; private set; } = new();

        private Action? _refreshUI;

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

            // Activités récentes simulées
            RecentActivities = new List<AdminActivity>
            {
                new AdminActivity
                {
                    Icon = "👤",
                    Description = "Nouvel utilisateur inscrit : Jean Dupont",
                    Type = "info",
                    TimeAgo = "Il y a 5 minutes"
                },
                new AdminActivity
                {
                    Icon = "🚗",
                    Description = "Nouvelle annonce publiée : BMW Série 3",
                    Type = "info",
                    TimeAgo = "Il y a 15 minutes"
                },
                new AdminActivity
                {
                    Icon = "🚩",
                    Description = "Nouveau signalement : Contenu suspect",
                    Type = "warning",
                    TimeAgo = "Il y a 32 minutes"
                },
                new AdminActivity
                {
                    Icon = "✅",
                    Description = "Annonce validée : Mercedes Classe A",
                    Type = "info",
                    TimeAgo = "Il y a 1 heure"
                },
                new AdminActivity
                {
                    Icon = "❌",
                    Description = "Compte suspendu : Violation des CGU",
                    Type = "danger",
                    TimeAgo = "Il y a 2 heures"
                }
            };

            _refreshUI?.Invoke();
        }

        public async Task RefreshData()
        {
            await LoadDashboardData();
        }
    }

    public class AdminActivity
    {
        public string Icon { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "info"; // info, warning, danger
        public string TimeAgo { get; set; } = "";
    }
}