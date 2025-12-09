using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class CommandeComposantViewModel
    {
        private readonly ICompteService _compteService;

        public bool IsFavorite { get; private set; }
        private int? _currentUserId;
        private Action? _refreshUI;

        public CommandeComposantViewModel(
            ICompteService compteService)
        {
            _compteService = compteService;
        }

        public async Task InitializeAsync(CommandeDTO commande, Action refreshUI)
        {
            _refreshUI = refreshUI;

            try
            {
                var compte = await _compteService.GetMe();
                _currentUserId = compte?.IdCompte;
            }
            catch
            {
                _currentUserId = null;
                IsFavorite = false;
            }
        }
    }
}