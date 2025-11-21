using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class GetAllViewModel
{
    private readonly IService<Marque> _marqueService;
    private readonly IModeleService _modeleService;
    private readonly IService<Carburant> _carburantService;
    private readonly IService<Categorie> _categorieService;
    private readonly IService<Motricite> _motriciteService;
    
    public Marque[] allMarques;
    public Modele[] allModeles;
    public Modele[] filteredModeles;
    public Carburant[] allCarburants;
    public Motricite[] allMotricite;
    public Categorie[] allCategories;
    
    public GetAllViewModel(
        IService<Marque> marqueService,
        IModeleService modeleService,
        IService<Carburant> carburantService,
        IService<Categorie> categorieService,
        IService<Motricite> _motriciteService)
    {
        _marqueService = marqueService;
        _modeleService = modeleService;
        _carburantService = carburantService;
        _categorieService = categorieService;
        _motriciteService = _motriciteService;
    }
    
    public async Task InitializeAsync()
    {
        allMarques = (await _marqueService.GetAllAsync()).ToArray();
        allModeles = (await _modeleService.GetAllAsync()).ToArray();
        filteredModeles = allModeles;

        allCarburants = (await _carburantService.GetAllAsync()).ToArray();
        allCategories = (await _categorieService.GetAllAsync()).ToArray();
        allMotricite = (await _motriciteService.GetAllAsync()).ToArray();
    }
}