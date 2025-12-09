namespace BlazorAutoPulse.ViewModel
{
    public class AdminAnnoncesViewModel
    {
        private List<AdminAnnonce> AllAnnonces { get; set; } = new();
        public List<AdminAnnonce> FilteredAnnonces { get; private set; } = new();

        public string SearchQuery { get; set; } = "";
        public string FilterStatus { get; private set; } = "all";

        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 9;
        public int TotalPages => (int)Math.Ceiling((double)FilteredAnnonces.Count / ItemsPerPage);
        public bool CanGoPrevious => CurrentPage > 1;
        public bool CanGoNext => CurrentPage < TotalPages;

        public int TotalAnnonces => AllAnnonces.Count;
        public int AnnoncesActives => AllAnnonces.Count(a => a.Statut == "Active");
        public int AnnoncesEnAttente => AllAnnonces.Count(a => a.Statut == "En attente");
        public int AnnoncesRefusees => AllAnnonces.Count(a => a.Statut == "Refusée");
        public int AnnoncesMisesEnAvant => AllAnnonces.Count(a => a.EstMiseEnAvant);

        public bool IsLoading { get; private set; } = true;

        private Action? _refreshUI;

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadAnnonces();
        }

        private async Task LoadAnnonces()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            await Task.Delay(500);

            AllAnnonces = new List<AdminAnnonce>
            {
                new AdminAnnonce
                {
                    Id = 1,
                    Titre = "BMW Série 3 320d",
                    Description = "Berline allemande en excellent état",
                    Prix = 25000,
                    Annee = 2019,
                    Kilometrage = 45000,
                    NomVendeur = "Marie Martin",
                    DatePublication = DateTime.Now.AddDays(-5),
                    Statut = "Active",
                    NombreVues = 234,
                    EstMiseEnAvant = true
                },
                new AdminAnnonce
                {
                    Id = 2,
                    Titre = "Mercedes Classe A 180",
                    Description = "Compacte premium, première main",
                    Prix = 28500,
                    Annee = 2020,
                    Kilometrage = 32000,
                    NomVendeur = "Sophie Garage",
                    DatePublication = DateTime.Now.AddDays(-2),
                    Statut = "Active",
                    NombreVues = 189,
                    EstMiseEnAvant = false
                },
                new AdminAnnonce
                {
                    Id = 3,
                    Titre = "Audi A4 2.0 TDI",
                    Description = "Break familial spacieux",
                    Prix = 31000,
                    Annee = 2021,
                    Kilometrage = 28000,
                    NomVendeur = "Jean Dupont",
                    DatePublication = DateTime.Now.AddHours(-3),
                    Statut = "En attente",
                    NombreVues = 12,
                    EstMiseEnAvant = false
                },
                new AdminAnnonce
                {
                    Id = 4,
                    Titre = "Renault Clio V E-Tech",
                    Description = "Citadine hybride récente",
                    Prix = 18500,
                    Annee = 2022,
                    Kilometrage = 15000,
                    NomVendeur = "Luc Bernard",
                    DatePublication = DateTime.Now.AddHours(-1),
                    Statut = "En attente",
                    NombreVues = 5,
                    EstMiseEnAvant = false
                },
                new AdminAnnonce
                {
                    Id = 5,
                    Titre = "Peugeot 3008 GT Line",
                    Description = "SUV français haut de gamme",
                    Prix = 35000,
                    Annee = 2021,
                    Kilometrage = 38000,
                    NomVendeur = "Sophie Garage",
                    DatePublication = DateTime.Now.AddDays(-7),
                    Statut = "Active",
                    NombreVues = 456,
                    EstMiseEnAvant = true
                },
                new AdminAnnonce
                {
                    Id = 6,
                    Titre = "Volkswagen Golf 8 GTI",
                    Description = "Sportive compacte iconique",
                    Prix = 38000,
                    Annee = 2022,
                    Kilometrage = 12000,
                    NomVendeur = "Pierre Durand",
                    DatePublication = DateTime.Now.AddDays(-15),
                    Statut = "Suspendue",
                    NombreVues = 312,
                    EstMiseEnAvant = false
                }
            };

            FilteredAnnonces = new List<AdminAnnonce>(AllAnnonces);
            IsLoading = false;
            _refreshUI?.Invoke();
        }

        public List<AdminAnnonce> GetPagedAnnonces()
        {
            return FilteredAnnonces
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();
        }

        public async Task FilterByStatus(string status)
        {
            FilterStatus = status;
            ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task SearchAnnonces()
        {
            ApplyFilters();
            CurrentPage = 1;
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        private void ApplyFilters()
        {
            FilteredAnnonces = AllAnnonces;

            if (FilterStatus != "all")
            {
                FilteredAnnonces = FilterStatus switch
                {
                    "active" => FilteredAnnonces.Where(a => a.Statut == "Active").ToList(),
                    "pending" => FilteredAnnonces.Where(a => a.Statut == "En attente").ToList(),
                    "rejected" => FilteredAnnonces.Where(a => a.Statut == "Refusée").ToList(),
                    "featured" => FilteredAnnonces.Where(a => a.EstMiseEnAvant).ToList(),
                    _ => FilteredAnnonces
                };
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                FilteredAnnonces = FilteredAnnonces.Where(a =>
                    a.Titre.ToLower().Contains(query) ||
                    a.Description.ToLower().Contains(query) ||
                    a.NomVendeur.ToLower().Contains(query)
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

        public async Task ViewAnnonceDetails(AdminAnnonce annonce)
        {
            Console.WriteLine($"Voir détails: {annonce.Titre}");
            await Task.CompletedTask;
        }

        public async Task ApproveAnnonce(AdminAnnonce annonce)
        {
            annonce.Statut = "Active";
            Console.WriteLine($"Annonce approuvée: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task RejectAnnonce(AdminAnnonce annonce)
        {
            annonce.Statut = "Refusée";
            Console.WriteLine($"Annonce refusée: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task SuspendAnnonce(AdminAnnonce annonce)
        {
            annonce.Statut = "Suspendue";
            Console.WriteLine($"Annonce suspendue: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task ReactivateAnnonce(AdminAnnonce annonce)
        {
            annonce.Statut = "Active";
            Console.WriteLine($"Annonce réactivée: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task FeatureAnnonce(AdminAnnonce annonce)
        {
            annonce.EstMiseEnAvant = true;
            Console.WriteLine($"Annonce mise en avant: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task UnfeatureAnnonce(AdminAnnonce annonce)
        {
            annonce.EstMiseEnAvant = false;
            Console.WriteLine($"Mise en avant retirée: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }

        public async Task DeleteAnnonce(AdminAnnonce annonce)
        {
            AllAnnonces.Remove(annonce);
            ApplyFilters();
            Console.WriteLine($"Annonce supprimée: {annonce.Titre}");
            _refreshUI?.Invoke();
            await Task.CompletedTask;
        }
    }

    public class AdminAnnonce
    {
        public int Id { get; set; }
        public string Titre { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Prix { get; set; }
        public int Annee { get; set; }
        public int Kilometrage { get; set; }
        public string NomVendeur { get; set; } = "";
        public DateTime DatePublication { get; set; }
        public string Statut { get; set; } = "";
        public int NombreVues { get; set; }
        public bool EstMiseEnAvant { get; set; }
        public bool IsSelected { get; set; }
    }
}