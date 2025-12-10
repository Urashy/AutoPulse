using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class AdminLayoutViewModel
    {
        private readonly ICompteService _compteService;
        private NavigationManager? _nav;

        private Action? _refreshUI;
        public AdminLayoutViewModel(ICompteService compteService)
        {
            _compteService = compteService;
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

                if(compte.IdTypeCompte != 1)
                {
                    _nav.NavigateTo("login");
                }
            }
            catch
            {
                _nav.NavigateTo("login");
            }
        }
    }
}
