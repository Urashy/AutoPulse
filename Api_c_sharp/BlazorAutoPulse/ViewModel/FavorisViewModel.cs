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
                AnnoncesFavoris.Clear();

                var me = await _compteService.GetMe();
                Favoris = await _favorisService.GetMesFavoris(me.IdCompte);

                var annonceIds = Favoris.Select(f => f.IdAnnonce).ToList();

                // Charger les annonces depuis GetByCompteID qui retourne maintenant des AnnonceDTO
                var allAnnonces = await _annonceService.GetByCompteID(me.IdCompte);

                // Filtrer pour ne garder que les favoris
                AnnoncesFavoris = allAnnonces.Where(a => annonceIds.Contains(a.IdAnnonce)).ToList();

                refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
                nav.NavigateTo("/connexion");
            }
            finally
            {
                IsLoading = false;
                refreshUI?.Invoke();
            }
        }
    }
}