using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;

namespace BlazorAutoPulse.ViewModel;

public class HomeViewModel
{
    private readonly IService<Annonce> _annonceService;
    
    public Annonce[] filteredAnnonces;
    public Annonce[] allAnnonces;
    
    private Action? _refreshUI;

    public HomeViewModel(IService<Annonce> annonceService)
    {
        _annonceService = annonceService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
        allAnnonces = (await _annonceService.GetAllAsync()).ToArray();
        await ReloadAnnonce();
    }

    public async Task ReloadAnnonce()
    {
        filteredAnnonces = null;
        await Task.Delay(100);
        //filteredAnnonces = (await _annonceService.GetMiseEnAvantAsync()).ToArray();
        _refreshUI?.Invoke();
    }
}