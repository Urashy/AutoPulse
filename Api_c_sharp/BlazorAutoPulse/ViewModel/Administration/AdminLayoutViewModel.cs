using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

public class AdminLayoutViewModel
{
    private readonly ICompteService _compteService;
    private readonly NavigationManager _nav;
    private Action? _refreshUI;

    public bool IsVerified { get; private set; } = false;
    public bool IsLoading { get; private set; } = true;
    public string CurrentPath { get; private set; } = string.Empty;

    public AdminLayoutViewModel(ICompteService compteService, NavigationManager nav)
    {
        _compteService = compteService;
        _nav = nav;

        _nav.LocationChanged += OnLocationChanged;
        UpdateCurrentPath();
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;
        await VerifUserAdmin();
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        UpdateCurrentPath();
        _refreshUI?.Invoke();
    }

    private void UpdateCurrentPath()
    {
        var uri = new Uri(_nav.Uri);
        CurrentPath = uri.AbsolutePath.ToLower();
    }

    public bool IsActive(string href)
    {
        if (href == "/admin" && CurrentPath == "/admin")
            return true;

        if (href != "/admin" && CurrentPath.StartsWith(href.ToLower()))
            return true;

        return false;
    }

    public string GetNavItemClass(string href)
    {
        return IsActive(href) ? "nav-item active" : "nav-item";
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