using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.ViewModel
{
    public class RechercheViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IService<MarqueDTO> _marqueService;
        private readonly IModeleService _modeleService;
        private readonly IService<CarburantDTO> _carburantService;
        private readonly IService<CategorieDTO> _categorieService;
        private readonly IService<BoiteDeVitesseDTO> _boiteService;
        private readonly ITypeCompteService _typecompteService;

        private Action? _refreshUI;

        // Propriétés pour les filtres
        public string SelectedMarque { get; set; } = "0";
        public string SelectedModele { get; set; } = "";
        public string SelectedCarburant { get; set; } = "0";
        public string SelectedCategorie { get; set; } = "0";
        public string SelectedType { get; set; } = "0";
        public string SelectedBoite { get; set; } = "0";
        public string Nom { get; set; } = "";
        public string Departement { get; set; } = "";
        public bool IsLoading { get; set; } = true;
        public int SelectedOrder { get; set; } = 4;


        // Propriétés pour les sliders (valeurs numériques)
        public int PrixMinValue { get; set; } = 0;
        public int PrixMaxValue { get; set; } = 200000;
        public int KmMinValue { get; set; } = 0;
        public int KmMaxValue { get; set; } = 300000;

        // Propriétés de pagination
        public int CurrentPage { get; private set; } = 1;
        public int ItemsPerPage { get; private set; } = 21;

        // Annonces de la page courante (utilise maintenant AnnonceDTO)
        public AnnonceDTO[] FilteredAnnonces { get; private set; } = Array.Empty<AnnonceDTO>();

        // Indicateur pour savoir s'il y a une page suivante
        private bool HasMorePages { get; set; } = true;

        // Nombre de résultats sur la page courante
        public int CurrentPageResultCount => FilteredAnnonces?.Length ?? 0;

        // Estimation du nombre total de pages
        public int EstimatedTotalPages => CalculateEstimatedTotalPages();

        // Anciennes propriétés string
        public string PrixMin => PrixMinValue > 0 ? PrixMinValue.ToString() : "";
        public string PrixMax => PrixMaxValue < 200000 ? PrixMaxValue.ToString() : "";
        public string KmMin => KmMinValue > 0 ? KmMinValue.ToString() : "";
        public string KmMax => KmMaxValue < 300000 ? KmMaxValue.ToString() : "";

        // Données
        public MarqueDTO[] AllMarques { get; private set; } = Array.Empty<MarqueDTO>();
        public ModeleDTO[] FilteredModeles { get; private set; } = Array.Empty<ModeleDTO>();
        public ModeleDTO[] AllModeles { get; private set; } = Array.Empty<ModeleDTO>();
        public CarburantDTO[] AllCarburants { get; private set; } = Array.Empty<CarburantDTO>();
        public CategorieDTO[] AllCategories { get; private set; } = Array.Empty<CategorieDTO>();
        public TypeCompteDTO[] AllTypeComptes { get; private set; } = Array.Empty<TypeCompteDTO>();
        public BoiteDeVitesseDTO[] AllBoitesDeVitesse { get; private set; } = Array.Empty<BoiteDeVitesseDTO>();

        // Propriétés calculées pour la pagination
        public string PaginationInfo => $"Page {CurrentPage} - {CurrentPageResultCount} résultat(s)";
        public bool CanGoToPreviousPage => CurrentPage > 1;
        public bool CanGoToNextPage => HasMorePages && CurrentPageResultCount == ItemsPerPage;
        public bool ShowPagination => CurrentPage > 1 || CanGoToNextPage;

        public RechercheViewModel(
            IAnnonceService annonceService,
            IService<MarqueDTO> marqueService,
            IModeleService modeleService,
            IService<CarburantDTO> carburantService,
            IService<CategorieDTO> categorieService,
            IService<BoiteDeVitesseDTO> boiteService,
            ITypeCompteService typecompteService)
        {
            _annonceService = annonceService;
            _marqueService = marqueService;
            _modeleService = modeleService;
            _carburantService = carburantService;
            _categorieService = categorieService;
            _boiteService= boiteService;
            _typecompteService = typecompteService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            IsLoading = true;

            try
            {
                AllMarques = (await _marqueService.GetAllAsync()).ToArray();
                AllModeles = (await _modeleService.GetAllAsync()).ToArray();
                FilteredModeles = AllModeles;
                AllCarburants = (await _carburantService.GetAllAsync()).ToArray();
                AllCategories = (await _categorieService.GetAllAsync()).ToArray();
                AllTypeComptes = (await _typecompteService.GetTypeComptesPourChercher()).ToArray();
                AllBoitesDeVitesse = (await _boiteService.GetAllAsync()).ToArray();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
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
            ResetToFirstPage();
            _refreshUI?.Invoke();

            await LoadPage();
        }

        private async Task LoadPage()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                var searchParams = BuildSearchParams();
                var results = (await _annonceService.GetFilteredAnnoncesAsync(searchParams)).ToArray();

                FilteredAnnonces = results;
                HasMorePages = results.Length == ItemsPerPage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la recherche: {ex.Message}");
                FilteredAnnonces = Array.Empty<AnnonceDTO>();
                HasMorePages = false;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        private ParametreRecherche BuildSearchParams()
        {
            return new ParametreRecherche
            {
                IdCarburant = SelectedCarburant != "0" ? int.Parse(SelectedCarburant) : 0,
                IdMarque = SelectedMarque != "0" ? int.Parse(SelectedMarque) : 0,
                IdModele = !string.IsNullOrEmpty(SelectedModele) ? int.Parse(SelectedModele) : 0,
                IdTypeVendeur = SelectedType != "0" ? int.Parse(SelectedType) : 0,
                PrixMin = PrixMinValue,
                PrixMax = PrixMaxValue < 200000 ? PrixMaxValue : 0,
                IdTypeVoiture = SelectedCategorie != "0" ? int.Parse(SelectedCategorie) : 0,
                Order = SelectedOrder,
                Nom = Nom ?? string.Empty,
                KmMin = KmMinValue,
                KmMax = KmMaxValue < 300000 ? KmMaxValue : 0,
                IdBoitedevitesse = SelectedBoite != "0" ? int.Parse(SelectedBoite) : 0,
                Departement = Departement ?? string.Empty,
                PageNumber = CurrentPage,
                PageSize = ItemsPerPage
            };
        }

        // Méthodes de pagination
        private int CalculateEstimatedTotalPages()
        {
            if (FilteredAnnonces == null || FilteredAnnonces.Length == 0) return 1;
            if (!HasMorePages && CurrentPage == 1) return 1;
            if (!HasMorePages) return CurrentPage;
            return CurrentPage + 1;
        }

        private void ResetToFirstPage()
        {
            CurrentPage = 1;
            HasMorePages = true;
        }

        public async Task GoToPage(int pageNumber)
        {
            if (IsValidPageNumber(pageNumber) && pageNumber != CurrentPage)
            {
                CurrentPage = pageNumber;
                await LoadPage();
            }
        }

        public async Task GoToNextPage()
        {
            if (CanGoToNextPage)
            {
                CurrentPage++;
                await LoadPage();
            }
        }

        public async Task GoToPreviousPage()
        {
            if (CanGoToPreviousPage)
            {
                CurrentPage--;
                await LoadPage();
            }
        }

        private bool IsValidPageNumber(int pageNumber)
        {
            return pageNumber >= 1;
        }

        public PaginationData GetPaginationData()
        {
            return new PaginationData
            {
                CurrentPage = CurrentPage,
                TotalPages = EstimatedTotalPages,
                TotalResults = CurrentPageResultCount,
                CanGoToPrevious = CanGoToPreviousPage,
                CanGoToNext = CanGoToNextPage,
                ShowFirstPage = ShouldShowFirstPage(),
                ShowLastPage = false,
                ShowFirstDots = ShouldShowFirstDots(),
                ShowLastDots = CanGoToNextPage && CurrentPage > 2,
                VisiblePages = GetVisiblePageNumbers()
            };
        }

        private bool ShouldShowFirstPage() => CurrentPage > 3;
        private bool ShouldShowFirstDots() => CurrentPage > 4;

        private int[] GetVisiblePageNumbers()
        {
            var totalPages = CalculateEstimatedTotalPages();

            var pages = new List<int>();
            var startPage = Math.Max(1, CurrentPage - 2);

            var endPage = Math.Min(totalPages, CurrentPage + 2);

            for (int i = startPage; i <= endPage; i++)
            {
                pages.Add(i);
            }

            return pages.ToArray();
        }

        public void ReinitialiserFiltres()
        {
            SelectedMarque = "0";
            SelectedModele = "";
            SelectedCarburant = "0";
            SelectedCategorie = "0";
            SelectedType = "0";
            PrixMinValue = 0;
            PrixMaxValue = 200000;
            KmMinValue = 0;
            KmMaxValue = 300000;
            SelectedBoite = "0";
            SelectedOrder = 4;  
            Nom = "";
            Departement = "";
            ResetToFirstPage();
            FilteredModeles = AllModeles;
            FilteredAnnonces = Array.Empty<AnnonceDTO>();
            HasMorePages = true;
            _refreshUI?.Invoke();
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

        public string GetTypeCompteLibelle(string idTypeCompte)
        {
            if (AllTypeComptes == null || string.IsNullOrEmpty(idTypeCompte)) return "";
            var typecompte = AllTypeComptes.FirstOrDefault(c => c.IdTypeCompte.ToString() == idTypeCompte);
            return typecompte?.Libelle ?? "";
        }

        public bool HasActiveFilters()
        {
            return SelectedMarque != "0"
                || !string.IsNullOrEmpty(SelectedModele)
                || SelectedCarburant != "0"
                || SelectedType != "0"
                || SelectedCategorie != "0"
                || PrixMinValue > 0
                || SelectedBoite != "0"
                || PrixMaxValue < 200000
                || SelectedOrder != 0
                || KmMinValue > 0
                || KmMaxValue < 300000
                || !string.IsNullOrEmpty(Nom)
                || !string.IsNullOrEmpty(Departement);
        }

        public string GetOrderLibelle(int order)
        {
            return order switch
            {
                1 => "Prix croissant",
                2 => "Prix décroissant",
                3 => "Date (plus ancien)",
                4 => "Date (plus récent)"
            };
        }

        public string GetBoiteLibelle(string idBoite)
        {
            if (AllBoitesDeVitesse == null || string.IsNullOrEmpty(idBoite) || idBoite == "0") return "";

            var boite = AllBoitesDeVitesse.FirstOrDefault(b => b.IdBoiteDeVitesse.ToString() == idBoite);

            return boite?.LibelleBoite ?? "";
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

            if (SelectedOrder != 0)
                queryParams.Add($"order={SelectedOrder}");

            if (PrixMinValue > 0)
                queryParams.Add($"prixmin={PrixMinValue}");

            if (PrixMaxValue < 200000)
                queryParams.Add($"prixmax={PrixMaxValue}");

            if (KmMinValue > 0)
                queryParams.Add($"kmmin={KmMinValue}");

            if (KmMaxValue < 300000)
                queryParams.Add($"kmmax={KmMaxValue}");

            if (!string.IsNullOrEmpty(Nom))
                queryParams.Add($"nom={Uri.EscapeDataString(Nom)}");

            if (!string.IsNullOrEmpty(Departement))
                queryParams.Add($"departement={Uri.EscapeDataString(Departement)}");

            return queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
        }
    }
}