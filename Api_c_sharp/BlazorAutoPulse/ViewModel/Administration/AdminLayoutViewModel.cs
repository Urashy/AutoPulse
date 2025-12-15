using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

public class AdminLayoutViewModel
{
    private readonly ICompteService _compteService;
    private readonly NavigationManager _nav;

    private Action? _refreshUI;

    public bool IsVerified { get; private set; } = false;
    public bool IsLoading { get; private set; } = true;

    public AdminLayoutViewModel(ICompteService compteService, NavigationManager nav)
    {
        _compteService = compteService;
        _nav = nav;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
        await VerifUserAdmin();
    }

    private async Task VerifUserAdmin()
    {
        IsLoading = true;
        _refreshUI?.Invoke();

        try
        {
            var compte = await _compteService.GetMe();

            if (compte.IdTypeCompte != 3)
            {
                _nav.NavigateTo("", forceLoad: true);
                return;
            }
            IsVerified = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur vérification admin: {ex.Message}");
            _nav.NavigateTo("connexion", forceLoad: true);
            return;
        }
        finally
        {
            IsLoading = false;
            _refreshUI?.Invoke();
        }
    }
}