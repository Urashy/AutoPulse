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
            try
            {
                var me = await _compteService.GetMe();
                Favoris = await _favorisService.GetMesFavoris(me.IdCompte);

                // Charger toutes les annonces en une seule fois
                var annonceIds = Favoris.Select(f => f.IdAnnonce).ToList();

                foreach (var id in annonceIds)
                {
                    try
                    {
                        var annonce = await _annonceService.GetByIdAsync(id);
                        if (annonce != null)
                        {
                            AnnoncesFavoris.Add(annonce);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erreur chargement annonce {id}: {ex.Message}");
                    }
                }

                refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
                // Rediriger vers connexion si non authentifié
                nav.NavigateTo("/connexion");
            }
        }
    }
}
