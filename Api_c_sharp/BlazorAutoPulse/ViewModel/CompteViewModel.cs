using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.ViewModel
{
    public class CompteViewModel
    {
        private readonly ICompteService _compteService;

        public Compte compte;
        private Action? _refreshUI;

        public CompteViewModel(ICompteService compteService)
        {
            _compteService = compteService;
        }
        
        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            
            compte = new Compte();
            compte = await _compteService.GetMe();
        }
    }
}
