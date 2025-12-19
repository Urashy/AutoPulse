using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel.Administration
{
    public class AdminPlainteViewModel
    {
        private readonly ICompteService _compteService;
        private readonly IPlainteService _plainteService;

        private IEnumerable<PlainteDTO> plaintes;
        public AdminPlainteViewModel(ICompteService compteService, IPlainteService plainteService)
        {
            _compteService = compteService;
            _plainteService = plainteService;
        }

        private Action? _refreshUI;

        public PlainteDTO? SelectedPlainte { get; set; }
        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            await LoadPlaintes();
        }

        private async Task LoadPlaintes()
        {
            plaintes = await _plainteService.GetAllAsync();
            _refreshUI?.Invoke();
        }

        public async Task RefuserPlainte()
        {

            PlainteUpdateDTO plaintemodifiee = Creerplainteupdate();
            plaintemodifiee.IdEtat = 3;
            await _plainteService.UpdatePlainteAsync(SelectedPlainte.IdPlainte, plaintemodifiee);
        }
        public async Task AccepterPlainte()
        {

            PlainteUpdateDTO plaintemodifiee = Creerplainteupdate();
            plaintemodifiee.IdEtat = 2;
            await _plainteService.UpdatePlainteAsync(SelectedPlainte.IdPlainte, plaintemodifiee);
        }
        public PlainteUpdateDTO Creerplainteupdate()
        {
            PlainteUpdateDTO plaintemodifiee = new PlainteUpdateDTO
            {
                Description = SelectedPlainte.Description,
                IdCompte = SelectedPlainte.IdCompte,
                IdSignalement = SelectedPlainte.IdSignalement   
            };
            return plaintemodifiee;
        }
    }
}
