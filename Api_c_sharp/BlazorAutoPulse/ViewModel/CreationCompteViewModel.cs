using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Authentification;

namespace BlazorAutoPulse.ViewModel;

public class CreationCompteViewModel
{
    private readonly IServiceConnexion _connexionService;

    public bool pro = false;
    
    public Compte compte { get; set; }
    
    private Action? _refreshUI;

    public CreationCompteViewModel(IServiceConnexion connexionService)
    {
        _connexionService  = connexionService;
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
        Console.WriteLine("Prénom : " + compte.Prenom);
        Console.WriteLine("Nom : " + compte.Nom);
        Console.WriteLine("Pseudo : " + compte.Pseudo);
        Console.WriteLine("MotDePasse : " + compte.MotDePasse);
        Console.WriteLine("Email : " + compte.Email);
        Console.WriteLine("DateNaissance : " + compte.DateNaissance);
        Console.WriteLine("Biographie : " + compte.Biographie);
        Console.WriteLine("IdTypeCompte : " + compte.IdTypeCompte);
        Console.WriteLine("NumeroSiret : " + compte.NumeroSiret);
        Console.WriteLine("RaisonSociale : " + compte.RaisonSociale);
        Console.WriteLine(_connexionService.CreateUser(compte));
    }

    public async Task ReloadPage()
    {
        pro = !pro;
        Console.WriteLine(pro);
        _refreshUI?.Invoke();
    }
}