using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class HomeViewModel
{
    // Service
    private readonly IAnnonceService _annonceService;
    private readonly IService<Marque> _marqueService;
    private readonly IModeleService _modeleService;
    private readonly IService<Carburant> _carburantService;
    private readonly IService<Categorie> _categorieService;
    private readonly IService<TypeCompte> _typeCompteService;

    // Paramètres de recherche
    public ParametreRecherche SearchParams { get; set; } = new();

    // Données
    public Annonce[] filteredAnnonces;
    public Annonce[] allAnnonces;
    public Marque[] allMarques;
    public Modele[] allModeles;
    public Modele[] filteredModeles;
    public Carburant[] allCarburants;
    public Categorie[] allCategories;
    public TypeCompte[] allTypesCompte;

    private Action? _refreshUI;

    public HomeViewModel(
        IAnnonceService annonceService,
        IService<Marque> marqueService,
        IModeleService modeleService,
        IService<Carburant> carburantService,
        IService<Categorie> categorieService,
        IService<TypeCompte> typeCompteService)
    {
        _annonceService = annonceService;
        _marqueService = marqueService;
        _modeleService = modeleService;
        _carburantService = carburantService;
        _categorieService = categorieService;
        _typeCompteService = typeCompteService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;

        // Charger toutes les données nécessaires
        allAnnonces = (await _annonceService.GetByIdMiseEnAvant(3)).ToArray();
        filteredAnnonces = allAnnonces;

        allMarques = (await _marqueService.GetAllAsync()).ToArray();
        allModeles = (await _modeleService.GetAllAsync()).ToArray();
        filteredModeles = allModeles;

        allCarburants = (await _carburantService.GetAllAsync()).ToArray();
        allCategories = (await _categorieService.GetAllAsync()).ToArray();
        //allTypesCompte = (await _typeCompteService.GetAllAsync()).ToArray();
    }

    public async Task FiltreModeleParMarque(ChangeEventArgs e)
    {
        int selectedMarqueId = int.Parse(e.Value?.ToString() ?? "0");
        SearchParams.IdMarque = selectedMarqueId;

        if (selectedMarqueId == 0)
        {
            filteredModeles = allModeles;
            SearchParams.IdModele = 0;
        }
        else
        {
            filteredModeles = (await _modeleService.FiltreModeleParMarque(selectedMarqueId)).ToArray();
        }

        _refreshUI?.Invoke();
    }

    public async Task RechercherAnnonces()
    {
        try
        {
            filteredAnnonces = (await _annonceService.GetFilteredAnnoncesAsync(SearchParams)).ToArray();
            _refreshUI?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de la recherche: {ex.Message}");
        }
    }

    public async Task ReinitialiserRecherche()
    {
        SearchParams = new ParametreRecherche();
        filteredModeles = allModeles;
        filteredAnnonces = allAnnonces;
        _refreshUI?.Invoke();
    }
}