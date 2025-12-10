using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminLayoutViewModel
    {
        private readonly ICompteService _compteService;
        private readonly NavigationManager _nav; 

        private Action? _refreshUI;
        public AdminLayoutViewModel(ICompteService compteService, NavigationManager nav)
        {
            _compteService = compteService;
            _nav = nav;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;

            verifUserAdmin();
        }

        private async void verifUserAdmin()
        {
            CompteDetailDTO compte;

            try
            {
                compte = await _compteService.GetMe();

                if (compte.IdTypeCompte != 3)
                {
                    _nav.NavigateTo(""); 
                }
            }
            catch
            {
                _nav.NavigateTo("connexion");
            }
        }
    }
}