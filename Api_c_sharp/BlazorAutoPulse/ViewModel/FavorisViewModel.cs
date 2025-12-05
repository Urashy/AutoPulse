using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class FavorisViewModel
    {
        private readonly ICompteService _compteService;
        private readonly IFavorisService _favorisService;
        private readonly IAnnonceService _annonceService;

        public List<AnnonceDTO> AnnoncesFavoris { get; set; } = new List<AnnonceDTO>();
        public IEnumerable<Favori> Favoris { get; set; }
        public bool IsLoading { get; set; } = true;

        public FavorisViewModel(
            ICompteService compteService,
            IFavorisService favorisservice,
            IAnnonceService annonceService)
        {
            _compteService = compteService;
            _favorisService = favorisservice;
            _annonceService = annonceService;
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            IsLoading = true;
            refreshUI?.Invoke();

            try
            {
                var me = await _compteService.GetMe();

                AnnoncesFavoris = (await _annonceService.GetAnnoncesFavoritesByCompteId(me.IdCompte)).ToList();

                refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
                //nav.NavigateTo("/connexion");
            }
            finally
            {
                IsLoading = false;
                refreshUI?.Invoke();
            }
        }
    }
}