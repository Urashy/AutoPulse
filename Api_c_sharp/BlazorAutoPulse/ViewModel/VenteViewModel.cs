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
        private readonly IService<APourCouleur> _aPourCouleurService;

        //-------------------------------- Modele
        public List<ImageUpload> imageUpload;
        
        public Annonce annonce;
        public Voiture voiture;
        public Adresse adresse;
        
        public List<string> nomPhotos { get; set; } = new();
        public List<int> selectedCouleurs { get; set; } = new();
        public bool dropdownOpen = false;

        // Gestion des erreurs
        public Dictionary<string, string> errors { get; set; } = new();
        public bool showErrors { get; set; } = false;

        private Action? _refreshUI;
        private NavigationManager _nav;

        public VenteViewModel(
            IAnnonceService annonceService, 
            IService<Voiture> voitureService, 
            IPostImageService postImageService,
            IService<Adresse> adresseService,
            IService<APourCouleur> aPourCouleurService)
        {
            _annonceService = annonceService;
            _voitureService = voitureService;
            _postImageService = postImageService;
            _adresseService = adresseService;
            _aPourCouleurService = aPourCouleurService;
            
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
            
            selectedCouleurs = new List<int>();
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
            
            if (errors.ContainsKey("photos"))
                errors.Remove("photos");

            _refreshUI?.Invoke();
        }

        public void OnMarqueChanged(ChangeEventArgs e)
        {
            voiture.IdMarque = int.Parse(e.Value.ToString());
            if (voiture.IdMarque != 0 && errors.ContainsKey("marque"))
                errors.Remove("marque");
            _refreshUI?.Invoke();
        }
        
        public void OnModeleChanged(ChangeEventArgs e)
        {
            voiture.IdModele = int.Parse(e.Value.ToString());
            if (voiture.IdModele != 0 && errors.ContainsKey("modele"))
                errors.Remove("modele");
        }

        public void OnCarburantChange(ChangeEventArgs e)
        {
            voiture.IdCarburant = int.Parse(e.Value.ToString());
            if (voiture.IdCarburant == 4)
            {
                voiture.IdBoiteDeVitesse = 2;
            }
            if (voiture.IdCarburant != 0 && errors.ContainsKey("carburant"))
                errors.Remove("carburant");
            _refreshUI?.Invoke();
        }

        public void OnMotriciteChange(ChangeEventArgs e)
        {
            voiture.IdMotricite = int.Parse(e.Value.ToString());
            if (voiture.IdMotricite != 0 && errors.ContainsKey("motricite"))
                errors.Remove("motricite");
        }
        
        public void OnBoiteDeVitesseChange(ChangeEventArgs e)
        {
            voiture.IdBoiteDeVitesse = int.Parse(e.Value.ToString());
            if (voiture.IdBoiteDeVitesse != 0 && errors.ContainsKey("boitedevitesse"))
                errors.Remove("boitedevitesse");
        }
        
        public void OnCategorieChange(ChangeEventArgs e)
        {
            voiture.IdCategorie = int.Parse(e.Value.ToString());
            if (voiture.IdCategorie != 0 && errors.ContainsKey("categorie"))
                errors.Remove("categorie");
        }
        
        public void ToggleCouleur(int idCouleur)
        {
            if (selectedCouleurs.Contains(idCouleur))
                selectedCouleurs.Remove(idCouleur);
            else
                selectedCouleurs.Add(idCouleur);
            
            if (selectedCouleurs.Any() && errors.ContainsKey("couleurs"))
                errors.Remove("couleurs");
            
            _refreshUI?.Invoke();
        }
        
        public void ToggleDropdown()
        {
            dropdownOpen = !dropdownOpen;
            _refreshUI?.Invoke();
        }

        public void CloseDropdown()
        {
            if (dropdownOpen)
            {
                dropdownOpen = false;
                _refreshUI?.Invoke();
            }
        }

        private bool ValidateForm()
        {
            errors.Clear();

            if (string.IsNullOrWhiteSpace(annonce.Libelle))
                errors.Add("titre", "Le titre est requis");

            if (!nomPhotos.Any())
                errors.Add("photos", "Au moins une photo est requise");

            if (voiture.IdMarque == null || voiture.IdMarque == 0)
                errors.Add("marque", "Veuillez sélectionner une marque");

            if (voiture.IdModele == null || voiture.IdModele == 0)
                errors.Add("modele", "Veuillez sélectionner un modèle");

            if (voiture.Annee == 0 || voiture.Annee < 1900 || voiture.Annee > DateTime.Now.Year + 1)
                errors.Add("annee", "Année invalide");

            if (voiture.Kilometrage < 0)
                errors.Add("kilometrage", "Kilométrage invalide");

            if (voiture.IdCarburant == null || voiture.IdCarburant == 0)
                errors.Add("carburant", "Veuillez sélectionner un carburant");

            if (voiture.IdMotricite == null || voiture.IdMotricite == 0)
                errors.Add("motricite", "Veuillez sélectionner une motricité");

            if (voiture.Puissance <= 0)
                errors.Add("puissance", "Puissance invalide");

            if (voiture.Couple <= 0)
                errors.Add("couple", "Couple invalide");

            if (voiture.NbPorte <= 0)
                errors.Add("nbporte", "Nombre de portes invalide");

            if (voiture.NbPlace <= 0)
                errors.Add("nbplace", "Nombre de places invalide");

            if (voiture.IdCarburant != 4 && voiture.NbCylindres <= 0)
                errors.Add("nbcylindres", "Nombre de cylindres invalide");

            if (voiture.IdBoiteDeVitesse == null || voiture.IdBoiteDeVitesse == 0)
                errors.Add("boitedevitesse", "Veuillez sélectionner une boîte de vitesse");

            if (voiture.IdCategorie == null || voiture.IdCategorie == 0)
                errors.Add("categorie", "Veuillez sélectionner une catégorie");

            if (annonce.Prix == null || annonce.Prix <= 0)
                errors.Add("prix", "Prix invalide");

            if (!selectedCouleurs.Any())
                errors.Add("couleurs", "Veuillez sélectionner au moins une couleur");

            if (string.IsNullOrWhiteSpace(adresse.Nom))
                errors.Add("nomadresse", "Le nom de l'adresse est requis");

            if (adresse.Numero == null || adresse.Numero <= 0)
                errors.Add("numeroadresse", "Numéro de rue invalide");

            if (string.IsNullOrWhiteSpace(adresse.Rue))
                errors.Add("rueadresse", "La rue est requise");

            if (string.IsNullOrWhiteSpace(adresse.CodePostal))
                errors.Add("codepostal", "Le code postal est requis");

            if (string.IsNullOrWhiteSpace(adresse.LibelleVille))
                errors.Add("ville", "La ville est requise");

            return !errors.Any();
        }

        public bool HasError(string fieldName)
        {
            return showErrors && errors.ContainsKey(fieldName);
        }

        public string GetError(string fieldName)
        {
            return errors.ContainsKey(fieldName) ? errors[fieldName] : "";
        }

        public async Task CreateAnnonce()
        {
            showErrors = true;

            if (!ValidateForm())
            {
                _refreshUI?.Invoke();
                return;
            }

            try
            {
                adresse.IdAdresse = 0;
                adresse.IdPays = 1;
                adresse.IdCompte = 1;
                Adresse resultAdr = await _adresseService.CreateAsync(adresse);
                Voiture resultVoiture = await _voitureService.CreateAsync(voiture);
                
                foreach (ImageUpload image in imageUpload)
                {
                    image.IdVoiture = resultVoiture.IdVoiture;
                    await _postImageService.CreateAsync(image);
                }
                
                foreach (int couleur in selectedCouleurs)
                {
                    APourCouleur aPourCouleur = new APourCouleur()
                    {
                        IdCouleur = couleur,
                        IdVoiture = resultVoiture.IdVoiture,
                    };
                    await _aPourCouleurService.CreateAsync(aPourCouleur);
                }
                
                annonce.IdAdresse = resultAdr.IdAdresse;
                annonce.IdVoiture = resultVoiture.IdVoiture;
                await _annonceService.CreateAsync(annonce);
                _nav.NavigateTo("/");

                voiture = new Voiture();
                adresse = new Adresse();
                annonce = new Annonce();
                nomPhotos = new List<string>();
                selectedCouleurs = new List<int>();
            }
            catch (Exception ex)
            {
                errors.Add("general", "Une erreur est survenue lors de la publication de l'annonce. Veuillez réessayer.");
                _refreshUI?.Invoke();
            }
        }
    }
}