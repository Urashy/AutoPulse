using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel;

public class CreationCompteViewModel
{
    private readonly IService<Compte> _compteService;

    public bool pro = false;
    
    public Compte compte { get; set; }
    
    private Action? _refreshUI;

    public CreationCompteViewModel(IService<Compte> compteService)
    {
        _compteService  = compteService;
        compte = new Compte();
        compte.DateNaissance = new DateTime(2000, 1, 1);
    }
    
    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
    }

    public async Task CreateCompteAsync()
    {
        compte.IdTypeCompte = (pro) ? 1 : 2;
        try
        {
            var createdCompte = await _compteService.CreateAsync(compte);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Erreur lors de la création : " + ex.Message);
        }
    }

    public async Task ReloadPage()
    {
        pro = !pro;
        Console.WriteLine(pro);
        _refreshUI?.Invoke();
    }
}