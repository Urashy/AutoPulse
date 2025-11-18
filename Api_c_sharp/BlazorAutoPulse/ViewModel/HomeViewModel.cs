using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel;

public class HomeViewModel
{
    //-------------------------------- Service
    private readonly IService<Annonce> _annonceService;
    private readonly IService<Marque> _marqueService;
    private readonly IModeleService _modeleService;

    //-------------------------------- Annocne
    public Annonce[] filteredAnnonces;
    public Annonce[] allAnnonces;

    //-------------------------------- Marque
    public Marque[] allMarques;

    //-------------------------------- modele
    public Modele[] allModeles;
    public Modele[] filteredModeles;

    private Action? _refreshUI;

    public HomeViewModel(IService<Annonce> annonceService, IService<Marque> marqueService, IModeleService modeleService)
    {
        _annonceService = annonceService;
        _marqueService = marqueService;
        _modeleService = modeleService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
        allAnnonces = (await _annonceService.GetAllAsync()).ToArray();
        allMarques = (await _marqueService.GetAllAsync()).ToArray();
        allModeles = (await _modeleService.GetAllAsync()).ToArray();
        filteredModeles = allModeles;
    }

    public async Task FiltreModeleParMarque(ChangeEventArgs e)
    {
        int selectedMarqueId = int.Parse(e.Value.ToString());

        if (selectedMarqueId == null || selectedMarqueId == 0)
        {
            filteredModeles = allModeles;
        }
        else
        {
            filteredModeles = (await _modeleService.FiltreModeleParMarque(selectedMarqueId)).ToArray();
        }

        _refreshUI?.Invoke();
    }
}