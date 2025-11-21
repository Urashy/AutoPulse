using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.ViewModel
{
    public class CompteViewModel
    {
        private readonly IService<Compte> _compteService;

        public Compte[] allComptes;
        private Action? _refreshUI;

        public CompteViewModel(IService<Compte> compteService)
            {
            _compteService = compteService;
            }
        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;

            
            allComptes = (await _compteService.GetAllAsync()).ToArray();
        }
    }
}
