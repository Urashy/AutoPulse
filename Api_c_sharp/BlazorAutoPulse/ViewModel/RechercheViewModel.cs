using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class RechercheViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IService<Marque> _marqueService;
        private readonly IModeleService _modeleService;
        private readonly IService<Carburant> _carburantService;
        private readonly IService<Categorie> _categorieService;

        private Action? _refreshUI;

        // Propriétés pour les filtres
        public string SelectedMarque { get; set; } = "0";
        public string SelectedModele { get; set; } = "";
        public string SelectedCarburant { get; set; } = "0";
        public string SelectedCategorie { get; set; } = "0";
        public string PrixMin { get; set; } = "";
        public string PrixMax { get; set; } = "";
        public string KmMin { get; set; } = "";
        public string KmMax { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Departement { get; set; } = "";
        public bool IsLoading { get; set; } = true;

        // Données
        public Annonce[] FilteredAnnonces { get; private set; } = Array.Empty<Annonce>();
        public Marque[] AllMarques { get; private set; } = Array.Empty<Marque>();
        public Modele[] FilteredModeles { get; private set; } = Array.Empty<Modele>();
        public Modele[] AllModeles { get; private set; } = Array.Empty<Modele>();
        public Carburant[] AllCarburants { get; private set; } = Array.Empty<Carburant>();
        public Categorie[] AllCategories { get; private set; } = Array.Empty<Categorie>();

        public RechercheViewModel(
            IAnnonceService annonceService,
            IService<Marque> marqueService,
            IModeleService modeleService,
            IService<Carburant> carburantService,
            IService<Categorie> categorieService)
        {
            _annonceService = annonceService;
            _marqueService = marqueService;
            _modeleService = modeleService;
            _carburantService = carburantService;
            _categorieService = categorieService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            IsLoading = true;

            try
            {
                // Charger les données de base
                AllMarques = (await _marqueService.GetAllAsync()).ToArray();
                AllModeles = (await _modeleService.GetAllAsync()).ToArray();
                FilteredModeles = AllModeles;
                AllCarburants = (await _carburantService.GetAllAsync()).ToArray();
                AllCategories = (await _categorieService.GetAllAsync()).ToArray();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task InitializeWithParams(string? marque, string? modele, string? prix)
        {
            SelectedMarque = marque ?? "0";
            SelectedModele = modele ?? "";
            PrixMax = prix ?? "";

            if (SelectedMarque != "0")
            {
                await FiltrerModeleParMarque(int.Parse(SelectedMarque));
            }

            await EffectuerRecherche();
        }

        public async Task OnMarqueChanged(ChangeEventArgs e)
        {
            SelectedMarque = e.Value?.ToString() ?? "0";
            SelectedModele = "";

            if (SelectedMarque == "0")
            {
                FilteredModeles = AllModeles;
            }
            else
            {
                await FiltrerModeleParMarque(int.Parse(SelectedMarque));
            }

            _refreshUI?.Invoke();
        }

        private async Task FiltrerModeleParMarque(int idMarque)
        {
            if (idMarque == 0)
            {
                FilteredModeles = AllModeles;
            }
            else
            {
                FilteredModeles = (await _modeleService.FiltreModeleParMarque(idMarque)).ToArray();
            }
        }

        public async Task EffectuerRecherche()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                var searchParams = new ParametreRecherche
                {
                    IdCarburant = SelectedCarburant != "0" ? int.Parse(SelectedCarburant) : 0,
                    IdMarque = SelectedMarque != "0" ? int.Parse(SelectedMarque) : 0,
                    IdModele = !string.IsNullOrEmpty(SelectedModele) ? int.Parse(SelectedModele) : 0,
                    PrixMin = !string.IsNullOrEmpty(PrixMin) ? int.Parse(PrixMin) : 0,
                    PrixMax = !string.IsNullOrEmpty(PrixMax) ? int.Parse(PrixMax) : 0,
                    IdTypeVoiture = SelectedCategorie != "0" ? int.Parse(SelectedCategorie) : 0,
                    Nom = Nom ?? string.Empty,
                    KmMin = !string.IsNullOrEmpty(KmMin) ? int.Parse(KmMin) : 0,
                    KmMax = !string.IsNullOrEmpty(KmMax) ? int.Parse(KmMax) : 0,
                    Departement = Departement ?? string.Empty
                };

                FilteredAnnonces = (await _annonceService.GetFilteredAnnoncesAsync(searchParams)).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche: {ex.Message}");
                FilteredAnnonces = Array.Empty<Annonce>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public void ReinitialiserFiltres()
        {
            SelectedMarque = "0";
            SelectedModele = "";
            SelectedCarburant = "0";
            SelectedCategorie = "0";
            PrixMin = "";
            PrixMax = "";
            KmMin = "";
            KmMax = "";
            Nom = "";
            Departement = "";
            FilteredModeles = AllModeles;
            FilteredAnnonces = Array.Empty<Annonce>();
        }

        public string GetMarqueLibelle(string idMarque)
        {
            if (AllMarques == null || string.IsNullOrEmpty(idMarque)) return "";
            var marque = AllMarques.FirstOrDefault(m => m.IdMarque.ToString() == idMarque);
            return marque?.LibelleMarque ?? "";
        }

        public string GetModeleLibelle(string idModele)
        {
            if (FilteredModeles == null || string.IsNullOrEmpty(idModele)) return "";
            var modele = FilteredModeles.FirstOrDefault(m => m.IdModele.ToString() == idModele);
            return modele?.LibelleModele ?? "";
        }

        public string GetCarburantLibelle(string idCarburant)
        {
            if (AllCarburants == null || string.IsNullOrEmpty(idCarburant)) return "";
            var carburant = AllCarburants.FirstOrDefault(c => c.IdCarburant.ToString() == idCarburant);
            return carburant?.LibelleCarburant ?? "";
        }

        public string GetCategorieLibelle(string idCategorie)
        {
            if (AllCategories == null || string.IsNullOrEmpty(idCategorie)) return "";
            var categorie = AllCategories.FirstOrDefault(c => c.IdCategorie.ToString() == idCategorie);
            return categorie?.LibelleCategorie ?? "";
        }

        public bool HasActiveFilters()
        {
            return SelectedMarque != "0"
                || !string.IsNullOrEmpty(SelectedModele)
                || SelectedCarburant != "0"
                || SelectedCategorie != "0"
                || !string.IsNullOrEmpty(PrixMin)
                || !string.IsNullOrEmpty(PrixMax)
                || !string.IsNullOrEmpty(KmMin)
                || !string.IsNullOrEmpty(KmMax)
                || !string.IsNullOrEmpty(Nom)
                || !string.IsNullOrEmpty(Departement);
        }

        public string BuildQueryString()
        {
            var queryParams = new List<string>();

            if (SelectedMarque != "0")
                queryParams.Add($"marque={SelectedMarque}");

            if (!string.IsNullOrEmpty(SelectedModele))
                queryParams.Add($"modele={SelectedModele}");

            if (SelectedCarburant != "0")
                queryParams.Add($"carburant={SelectedCarburant}");

            if (SelectedCategorie != "0")
                queryParams.Add($"categorie={SelectedCategorie}");

            if (!string.IsNullOrEmpty(PrixMin))
                queryParams.Add($"prixmin={PrixMin}");

            if (!string.IsNullOrEmpty(PrixMax))
                queryParams.Add($"prixmax={PrixMax}");

            if (!string.IsNullOrEmpty(KmMin))
                queryParams.Add($"kmmin={KmMin}");

            if (!string.IsNullOrEmpty(KmMax))
                queryParams.Add($"kmmax={KmMax}");

            if (!string.IsNullOrEmpty(Nom))
                queryParams.Add($"nom={Uri.EscapeDataString(Nom)}");

            if (!string.IsNullOrEmpty(Departement))
                queryParams.Add($"departement={Uri.EscapeDataString(Departement)}");

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        }
    }
}