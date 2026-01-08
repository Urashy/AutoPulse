using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class GetAllViewModel
{
    private readonly IService<MarqueDTO> _marqueService;
    private readonly IModeleService _modeleService;
    private readonly IService<CarburantDTO> _carburantService;
    private readonly IService<CategorieDTO> _categorieService;
    private readonly IService<MotriciteDTO> _motriciteService;
    private readonly IService<BoiteDeVitesseDTO> _boiteVitesseService;
    private readonly ICouleurService _couleurService;
    private readonly IService<MiseEnAvantDTO> _miseEnAvantService;
    
    public MarqueDTO[] allMarques;
    public ModeleDTO[] allModeles;
    public ModeleDTO[] filteredModeles;
    public CarburantDTO[] allCarburants;
    public MotriciteDTO[] allMotricite;
    public CategorieDTO[] allCategories;
    public BoiteDeVitesseDTO[] allBoiteDeVitesse;
    public CouleurDTO[] allCouleurs;
    public MiseEnAvantDTO[] allMiseEnAvant;
    
    public GetAllViewModel(
        IService<MarqueDTO> marqueService,
        IModeleService modeleService,
        IService<CarburantDTO> carburantService,
        IService<CategorieDTO> categorieService,
        IService<MotriciteDTO> motriciteService,
        IService<BoiteDeVitesseDTO> boiteVitesseService,
        ICouleurService couleurService,
        IService<MiseEnAvantDTO> miseEnAvantService)
    {
        _marqueService = marqueService;
        _modeleService = modeleService;
        _carburantService = carburantService;
        _categorieService = categorieService;
        _motriciteService = motriciteService;
        _boiteVitesseService = boiteVitesseService;
        _couleurService = couleurService;
        _miseEnAvantService = miseEnAvantService;
    }
    
    public async Task InitializeAsync()
    {
        allMarques = (await _marqueService.GetAllAsync()).ToArray();
        allModeles = (await _modeleService.GetAllAsync()).ToArray();
        filteredModeles = allModeles;

        allCarburants = (await _carburantService.GetAllAsync()).ToArray();
        allCategories = (await _categorieService.GetAllAsync()).ToArray();
        allMotricite = (await _motriciteService.GetAllAsync()).ToArray();
        allBoiteDeVitesse = (await _boiteVitesseService.GetAllAsync()).ToArray();
        allCouleurs = (await _couleurService.GetAllAsync()).ToArray();
        allMiseEnAvant = (await _miseEnAvantService.GetAllAsync()).ToArray();
    }
    
    public async Task OnMarqueChanged(int marqueId)
    {
        string SelectedMarque = marqueId.ToString() ?? "0";
        string SelectedModele = "";

        if (SelectedMarque == "0")
        {
            filteredModeles = allModeles;
        }
        else
        {
            await FiltrerModeleParMarque(int.Parse(SelectedMarque));
        }
    }

    private async Task FiltrerModeleParMarque(int idMarque)
    {
        if (idMarque == 0)
        {
            filteredModeles = allModeles;
        }
        else
        {
            filteredModeles = (await _modeleService.FiltreModeleParMarque(idMarque)).ToArray();
        }
    }

    // ✨ NOUVELLE MÉTHODE PUBLIQUE pour l'IA CNN
    /// <summary>
    /// Version publique de FiltrerModeleParMarque pour l'utilisation par l'IA
    /// </summary>
    public async Task FiltrerModeleParMarquePublic(int idMarque)
    {
        await FiltrerModeleParMarque(idMarque);
    }
}