using AutoPulse.Shared.DTO;
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
    private readonly IService<BoiteDeVitesse> _boiteVitesseService;
    private readonly ICouleurService _couleurService;
    
    public Marque[] allMarques;
    public Modele[] allModeles;
    public Modele[] filteredModeles;
    public Carburant[] allCarburants;
    public Motricite[] allMotricite;
    public Categorie[] allCategories;
    public BoiteDeVitesse[] allBoiteDeVitesse;
    public CouleurDTO[] allCouleurs;
    
    public GetAllViewModel(
        IService<Marque> marqueService,
        IModeleService modeleService,
        IService<Carburant> carburantService,
        IService<Categorie> categorieService,
        IService<Motricite> motriciteService,
        IService<BoiteDeVitesse> boiteVitesseService,
        ICouleurService couleurService)
    {
        _marqueService = marqueService;
        _modeleService = modeleService;
        _carburantService = carburantService;
        _categorieService = categorieService;
        _motriciteService = motriciteService;
        _boiteVitesseService = boiteVitesseService;
        _couleurService = couleurService;
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
    }
    
    public async Task OnMarqueChanged(ChangeEventArgs e)
    {
        string SelectedMarque = e.Value?.ToString() ?? "0";
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