using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using static System.Net.WebRequestMethods;

namespace BlazorAutoPulse.ViewModel
{
    public class VenteViewModel
    {
        //-------------------------------- Service
        private readonly IAnnonceService _annonceService;
        private readonly IService<Voiture> _voitureService;
        private readonly IService<Adresse> _adresseService;
        private readonly IPostImageService _postImageService;

        //-------------------------------- Modele
        public List<ImageUpload> imageUpload;
        
        public Annonce annonce;
        public Voiture voiture;
        public Adresse adresse;
        
        public List<string> nomPhotos { get; set; } = new();

        private Action? _refreshUI;
        private NavigationManager _nav;

        public VenteViewModel(
            IAnnonceService annonceService, 
            IService<Voiture> voitureService, 
            IPostImageService postImageService,
            IService<Adresse> adresseService)
        {
            _annonceService = annonceService;
            _voitureService = voitureService;
            _postImageService = postImageService;
            _adresseService = adresseService;
            imageUpload = new List<ImageUpload>();
            
            annonce = new Annonce()
            {
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdMiseEnAvant = 1,
                DatePublication = DateTime.Now,
                Annee = null,
                Kilometrage = null,
            };
            voiture = new Voiture
            {
                IdVoiture = 0,
                IdModeleBlender = null,

                Kilometrage = 0,
                Annee = 0,
                Puissance = 0,
                Couple = 0,
                NbCylindres = 0,
                MiseEnCirculation = DateTime.Now
            };
            voiture.MiseEnCirculation = DateTime.Now;
            adresse = new Adresse();
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
        }

        public async Task UploadImage(InputFileChangeEventArgs e)
        {
            foreach (var file in e.GetMultipleFiles())
            {
                nomPhotos.Add(file.Name);
                ImageUpload image = new ImageUpload();
                image.File = file;
                imageUpload.Add(image);
            }

            _refreshUI?.Invoke();
        }

        public void OnMarqueChanged(ChangeEventArgs e)
        {
            voiture.IdMarque = int.Parse(e.Value.ToString());
        }
        
        public void OnModeleChanged(ChangeEventArgs e)
        {
            voiture.IdModele = int.Parse(e.Value.ToString());
        }

        public void OnCarburantChange(ChangeEventArgs e)
        {
            voiture.IdCarburant = int.Parse(e.Value.ToString());
            if (voiture.IdCarburant == 4)
            {
                voiture.IdBoiteDeVitesse = 2;
            }
            _refreshUI?.Invoke();
        }

        public void OnMotriciteChange(ChangeEventArgs e)
        {
            voiture.IdMotricite = int.Parse(e.Value.ToString());
        }
        
        public void OnBoiteDeVitesseChange(ChangeEventArgs e)
        {
            voiture.IdBoiteDeVitesse = int.Parse(e.Value.ToString());
        }
        
        public void OnCategorieChange(ChangeEventArgs e)
        {
            voiture.IdCategorie = int.Parse(e.Value.ToString());
        }
        
        public void OnCouleurChange(ChangeEventArgs e)
        {
            voiture.IdCouleur = int.Parse(e.Value.ToString());
        }
        
        public async Task CreateAnnonce()
        {
            foreach (var image in imageUpload)
            {
                await _postImageService.CreateAsync(image);
            }
            adresse.IdAdresse = 0;
            adresse.IdPays = 1;
            adresse.IdCompte = 1;
            var resultAdr = await _adresseService.CreateAsync(adresse);
            var resultVoiture = await _voitureService.CreateAsync(voiture);
            annonce.IdAdresse = resultAdr.IdAdresse;
            annonce.IdVoiture = resultVoiture.IdVoiture;
            var resultAnnonce = await _annonceService.CreateAsync(annonce);
            _nav.NavigateTo("/");
            _refreshUI?.Invoke();
        }
    }
}