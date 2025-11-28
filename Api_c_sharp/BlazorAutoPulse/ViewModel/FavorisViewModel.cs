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

        public List<Annonce> AnnoncesFavoris { get; set; } = new List<Annonce>();
        public IEnumerable<Favori> Favoris { get; set; }

        public FavorisViewModel(ICompteService compteService, IFavorisService favorisservice, IAnnonceService annonceService)
        {
            _compteService = compteService;
            _favorisService = favorisservice;
            _annonceService = annonceService;
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            var me = await _compteService.GetMe();
            Favoris = await _favorisService.GetMesFavoris(me.IdCompte);

            foreach (var favori in Favoris)
            {
                var annonce = await _annonceService.GetByIdAsync(favori.IdAnnonce);
                AnnoncesFavoris.Add(annonce);
                Console.WriteLine(annonce.Libelle);
            }
        }
    }
}
