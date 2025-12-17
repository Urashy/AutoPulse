using System.Text.RegularExpressions;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using AutoPulse.Shared.DTO;
using VoitureDetailDTO = AutoPulse.Shared.DTO.VoitureDetailDTO;

namespace BlazorAutoPulse.ViewModel
{
    public class VenteViewModel
    {
        //-------------------------------- Services
        private readonly ICompteService _compteService;
        private readonly IAnnonceService _annonceService;
        private readonly IVoitureService _voitureService;
        private readonly IAdresseService _adresseService;
        private readonly IPostImageService _postImageService;
        private readonly IService<APourCouleur> _aPourCouleurService;
        private readonly IIAService _iaService;

        //-------------------------------- Modèles
        public List<ImageUpload> imageUpload;
        public List<AdresseDTO> compteAdresses;
        
        public AnnonceCreateDTO annonce;
        public VoitureDetailDTO VoitureDetailDto;
        public Adresse adresse;
        public int? selectedAddressId { get; set; } = null;
        
        public List<string> nomPhotos { get; set; } = new();
        public List<int> selectedCouleurs { get; set; } = new();
        public bool dropdownOpen = false;

        // Gestion des erreurs
        public Dictionary<string, string> errors { get; set; } = new();
        public bool showErrors { get; set; } = false;

        // États des popups IA
        public bool showCnnWarningPopup { get; set; } = false;
        public bool showCnnLoadingPopup { get; set; } = false;
        public bool showCnnResultPopup { get; set; } = false;
        public bool showPriceWarningPopup { get; set; } = false;
        public bool showPriceLoadingPopup { get; set; } = false;
        public bool showAdjustmentWarningPopup { get; set; } = false;
        public bool showAdjustmentLoadingPopup { get; set; } = false;
        public bool showAdjustmentResultPopup { get; set; } = false;

        // Résultats IA
        public ResultatCNN? cnnResult { get; set; }
        public ResultatPrediction? priceResult { get; set; }
        public ResultatAjustement? adjustmentResult { get; set; }

        // Liste des champs manquants pour la prédiction
        public List<string> missingFields { get; set; } = new();

        private Action? _refreshUI;
        private NavigationManager _nav;
        private GetAllViewModel _vmAll;

        public VenteViewModel(
            ICompteService compteService,
            IAnnonceService annonceService, 
            IVoitureService voitureService, 
            IPostImageService postImageService,
            IAdresseService adresseService,
            IService<APourCouleur> aPourCouleurService,
            IIAService iaService)
        {
            _compteService = compteService;
            _annonceService = annonceService;
            _voitureService = voitureService;
            _postImageService = postImageService;
            _adresseService = adresseService;
            _aPourCouleurService = aPourCouleurService;
            _iaService = iaService;
            
            imageUpload = new List<ImageUpload>();
            
            annonce = new AnnonceCreateDTO()
            {
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdMiseEnAvant = 1,
                DatePublication = DateTime.Now,
            };
            VoitureDetailDto = new VoitureDetailDTO
            {
                IdVoiture = 0,
                IdModeleBlender = null,
                Kilometrage = 0,
                Annee = 0,
                Puissance = 0,
                Couple = 0,
                NbCylindres = 0,
                MiseEnCirculation = DateTime.Now,
                InterieurCuire = false,
                CylindrerMoteur = 0,
                PositionVolant = true
            };
            adresse = new Adresse();
            selectedCouleurs = new List<int>();
        }

        public async Task InitializeAsync(Action refreshUI, NavigationManager nav, GetAllViewModel vmAll)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            _vmAll = vmAll;

            try
            {
                CompteDetailDTO compte = await _compteService.GetMe();
                compteAdresses = (await _adresseService.GetAdresseByCompte(compte.IdCompte)).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'initialisation: {ex.Message}");
            }
        }

        // ============================================================================
        // MÉTHODES IA - CNN (Reconnaissance visuelle)
        // ============================================================================

        public bool CanUseCnn => imageUpload.Any();

        public void OpenCnnWarningPopup()
        {
            if (!CanUseCnn) return;
            showCnnWarningPopup = true;
            _refreshUI?.Invoke();
        }

        public void CloseCnnWarningPopup()
        {
            showCnnWarningPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task ExecuteCnnRecognition()
        {
            showCnnWarningPopup = false;
            showCnnLoadingPopup = true;
            _refreshUI?.Invoke();

            try
            {
                // Convertir la première image en base64
                var firstImage = imageUpload.First();
                using var memoryStream = new MemoryStream();
                await firstImage.File.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024).CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                var dataCnn = new DataCNN
                {
                    ImageBase64 = base64Image
                };

                var result = await _iaService.PredictAIAsync(dataCnn);
                Console.WriteLine($"Type reçu: {result?.GetType().Name}");

                if (result is ResultatPrediction prediction)
                {
                    priceResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatPrediction");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatPrediction");
                }

                showCnnLoadingPopup = false;
                
                if (cnnResult.Success)
                {
                    showCnnResultPopup = true;
                }
                else
                {
                    errors.Add("cnn", cnnResult.Error ?? "Erreur lors de la reconnaissance");
                }
            }
            catch (Exception ex)
            {
                showCnnLoadingPopup = false;
                errors.Add("cnn", $"Erreur: {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        public async Task ApplyCnnResult()
        {
            if (cnnResult == null || !cnnResult.Success) return;

            // Rechercher la marque
            var marque = _vmAll.allMarques?.FirstOrDefault(m => 
                m.LibelleMarque.ToLower() == cnnResult.Manufacturer.ToLower());
            
            if (marque != null)
            {
                VoitureDetailDto.IdMarque = marque.IdMarque;
                
                // Utiliser la méthode de filtrage existante de VMAll
                await _vmAll.FiltrerModeleParMarquePublic(marque.IdMarque);
                
                // Rechercher le modèle dans les modèles filtrés
                var modele = _vmAll.filteredModeles?.FirstOrDefault(m => 
                    m.LibelleModele.Equals(cnnResult.Model, StringComparison.OrdinalIgnoreCase));
                
                if (modele != null)
                {
                    VoitureDetailDto.IdModele = modele.IdModele;
                }
            }

            showCnnResultPopup = false;
            _refreshUI?.Invoke();
        }

        public void CancelCnnResult()
        {
            showCnnResultPopup = false;
            _refreshUI?.Invoke();
        }

        // ============================================================================
        // MÉTHODES IA - Prédiction de prix
        // ============================================================================

        public void CheckAndPredictPrice()
        {
            missingFields.Clear();

            // Vérifier les champs requis
            if (VoitureDetailDto.IdMarque == null || VoitureDetailDto.IdMarque == 0)
                missingFields.Add("Marque");
            if (VoitureDetailDto.IdModele == null || VoitureDetailDto.IdModele == 0)
                missingFields.Add("Modèle");
            if (VoitureDetailDto.Annee == 0)
                missingFields.Add("Année");
            if (VoitureDetailDto.IdCategorie == null || VoitureDetailDto.IdCategorie == 0)
                missingFields.Add("Catégorie");
            if (VoitureDetailDto.IdCarburant == null || VoitureDetailDto.IdCarburant == 0)
                missingFields.Add("Carburant");
            if (VoitureDetailDto.Kilometrage == 0)
                missingFields.Add("Kilométrage");
            if (VoitureDetailDto.IdBoiteDeVitesse == null || VoitureDetailDto.IdBoiteDeVitesse == 0)
                missingFields.Add("Boîte de vitesse");
            if (VoitureDetailDto.IdMotricite == null || VoitureDetailDto.IdMotricite == 0)
                missingFields.Add("Motricité");

            if (missingFields.Any())
            {
                showPriceWarningPopup = true;
            }
            else
            {
                _ = ExecutePricePrediction();
            }

            _refreshUI?.Invoke();
        }

        public void ClosePriceWarningPopup()
        {
            showPriceWarningPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task ExecutePricePrediction()
        {
            showPriceWarningPopup = false;
            showPriceLoadingPopup = true;
            _refreshUI?.Invoke();
            

            try
            {
                var dataPrediction = new DataPrediction
                {
                    Manufacturer = GetMarqueLibelle(),
                    Model = GetModeleLibelle(),
                    ProdYear = VoitureDetailDto.Annee,
                    Category = GetCategorieLibelle(),
                    LeatherInterior = VoitureDetailDto.InterieurCuire ? "Yes" : "No",
                    FuelType = GetCarburantLibelle(),
                    EngineVolume = (float)VoitureDetailDto.CylindrerMoteur,
                    Mileage = VoitureDetailDto.Kilometrage.ToString(),
                    Cylinders = VoitureDetailDto.NbCylindres > 0 ? (float)VoitureDetailDto.NbCylindres : 4f,
                    GearBoxType = GetBoiteLibelle(),
                    DriveWheels = GetMotriciteLibelle(),
                    Doors = VoitureDetailDto.NbPorte.ToString() ?? "4", // Valeur par défaut
                    Wheel = VoitureDetailDto.PositionVolant ? "Left wheel" : "Right wheel",
                    Color = selectedCouleurs.Any() ? GetFirstCouleurLibelle() : "Black",
                    Airbags = 4, // Valeur par défaut - Non disponible dans le formulaire
                    Levy = 0f // Valeur par défaut - Non disponible dans le formulaire
                };

                var result = await _iaService.PredictAIAsync(dataPrediction);
                Console.WriteLine($"Type reçu: {result?.GetType().Name}");

                if (result is ResultatPrediction prediction)
                {
                    priceResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatPrediction");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatPrediction");
                }

                showPriceLoadingPopup = false;

                if (!priceResult.Success)
                {
                    errors.Add("price", priceResult.Error ?? "Erreur lors de la prédiction");
                }
            }
            catch (Exception ex)
            {
                showPriceLoadingPopup = false;
                errors.Add("price", $"Erreur: {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        public void AcceptPredictPrice()
        {
            annonce.Prix = (int)Math.Round(priceResult.PredictedPrice);
            _refreshUI?.Invoke();
        }

        // ============================================================================
        // MÉTHODES IA - Ajustement de prix
        // ============================================================================

        public bool CanUseAdjustment => !string.IsNullOrWhiteSpace(annonce.Description) && annonce.Prix > 0;

        public void OpenAdjustmentWarningPopup()
        {
            if (!CanUseAdjustment) return;
            showAdjustmentWarningPopup = true;
            _refreshUI?.Invoke();
        }

        public void CloseAdjustmentWarningPopup()
        {
            showAdjustmentWarningPopup = false;
            _refreshUI?.Invoke();
        }

        public async Task ExecuteAdjustment()
        {
            showAdjustmentWarningPopup = false;
            showAdjustmentLoadingPopup = true;
            _refreshUI?.Invoke();

            try
            {
                var dataAdjustment = new DataAjustement
                {
                    BasePrice = annonce.Prix,
                    Description = annonce.Description,
                };

                var result = await _iaService.PredictAIAsync(dataAdjustment);
                Console.WriteLine($"Type reçu: {result?.GetType().Name}");

                if (result is ResultatPrediction prediction)
                {
                    priceResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatPrediction");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatPrediction");
                }

                showAdjustmentLoadingPopup = false;

                if (adjustmentResult.Success)
                {
                    showAdjustmentResultPopup = true;
                }
                else
                {
                    errors.Add("adjustment", adjustmentResult.Error ?? "Erreur lors de l'ajustement");
                }
            }
            catch (Exception ex)
            {
                showAdjustmentLoadingPopup = false;
                errors.Add("adjustment", $"Erreur: {ex.Message}");
            }

            _refreshUI?.Invoke();
        }

        public void AcceptPriceAdjustment()
        {
            annonce.Prix = (int)Math.Round(adjustmentResult.AdjustedPrice);
            ClosePriceWarningPopup();
        }

        public void CloseAdjustmentResultPopup()
        {
            showAdjustmentResultPopup = false;
            _refreshUI?.Invoke();
        }

        // ============================================================================
        // MÉTHODES HELPER POUR RÉCUPÉRER LES LIBELLÉS
        // ============================================================================

        private string GetMarqueLibelle()
        {
            var marque = _vmAll?.allMarques?.FirstOrDefault(m => m.IdMarque == VoitureDetailDto.IdMarque);
            return marque?.LibelleMarque ?? "Unknown";
        }

        private string GetModeleLibelle()
        {
            var modele = _vmAll?.allModeles?.FirstOrDefault(m => m.IdModele == VoitureDetailDto.IdModele);
            return modele?.LibelleModele ?? "Unknown";
        }

        private string GetCategorieLibelle()
        {
            var categorie = _vmAll?.allCategories?.FirstOrDefault(c => c.IdCategorie == VoitureDetailDto.IdCategorie);
            return categorie?.LibelleCategorie ?? "Unknown";
        }

        private string GetCarburantLibelle()
        {
            var carburant = _vmAll?.allCarburants?.FirstOrDefault(c => c.IdCarburant == VoitureDetailDto.IdCarburant);
            return carburant?.LibelleCarburant ?? "Unknown";
        }

        private string GetBoiteLibelle()
        {
            var boite = _vmAll?.allBoiteDeVitesse?.FirstOrDefault(b => b.IdBoiteDeVitesse == VoitureDetailDto.IdBoiteDeVitesse);
            return boite?.LibelleBoite ?? "Unknown";
        }

        private string GetMotriciteLibelle()
        {
            var motricite = _vmAll?.allMotricite?.FirstOrDefault(m => m.IdMotricite == VoitureDetailDto.IdMotricite);
            return motricite?.LibelleMotricite ?? "Unknown";
        }

        private string GetFirstCouleurLibelle()
        {
            if (!selectedCouleurs.Any()) return "Black";
            
            var couleur = _vmAll?.allCouleurs?.FirstOrDefault(c => c.IdCouleur == selectedCouleurs.First());
            return couleur?.LibelleCouleur ?? "Black";
        }

        // ============================================================================
        // MÉTHODES EXISTANTES (inchangées)
        // ============================================================================

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
            VoitureDetailDto.IdMarque = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdMarque != 0 && errors.ContainsKey("marque"))
                errors.Remove("marque");
            _refreshUI?.Invoke();
        }
        
        public void OnModeleChanged(ChangeEventArgs e)
        {
            VoitureDetailDto.IdModele = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdModele != 0 && errors.ContainsKey("modele"))
                errors.Remove("modele");
        }

        public void OnCarburantChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdCarburant = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdCarburant == 4)
            {
                VoitureDetailDto.IdBoiteDeVitesse = 2;
                VoitureDetailDto.NbCylindres = 0;
                VoitureDetailDto.CylindrerMoteur = 0;
            }
            if (VoitureDetailDto.IdCarburant != 0 && errors.ContainsKey("carburant"))
                errors.Remove("carburant");
            _refreshUI?.Invoke();
        }

        public void OnMotriciteChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdMotricite = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdMotricite != 0 && errors.ContainsKey("motricite"))
                errors.Remove("motricite");
        }
        
        public void OnBoiteDeVitesseChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdBoiteDeVitesse = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdBoiteDeVitesse != 0 && errors.ContainsKey("boitedevitesse"))
                errors.Remove("boitedevitesse");
        }
        
        public void OnCategorieChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdCategorie = int.Parse(e.Value.ToString());
            if (VoitureDetailDto.IdCategorie != 0 && errors.ContainsKey("categorie"))
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

            if (VoitureDetailDto.IdMarque == null || VoitureDetailDto.IdMarque == 0)
                errors.Add("marque", "Veuillez sélectionner une marque");

            if (VoitureDetailDto.IdModele == null || VoitureDetailDto.IdModele == 0)
                errors.Add("modele", "Veuillez sélectionner un modèle");

            if (VoitureDetailDto.Annee == 0 || VoitureDetailDto.Annee < 1900 || VoitureDetailDto.Annee > DateTime.Now.Year + 1)
                errors.Add("annee", "Année invalide");

            if (VoitureDetailDto.Kilometrage < 0)
                errors.Add("kilometrage", "Kilométrage invalide");

            if (VoitureDetailDto.IdCarburant == null || VoitureDetailDto.IdCarburant == 0)
                errors.Add("carburant", "Veuillez sélectionner un carburant");

            if (VoitureDetailDto.IdMotricite == null || VoitureDetailDto.IdMotricite == 0)
                errors.Add("motricite", "Veuillez sélectionner une motricité");

            if (VoitureDetailDto.Puissance <= 0)
                errors.Add("puissance", "Puissance invalide");

            if (VoitureDetailDto.Couple <= 0)
                errors.Add("couple", "Couple invalide");

            if (VoitureDetailDto.NbPorte <= 0)
                errors.Add("nbporte", "Nombre de portes invalide");

            if (VoitureDetailDto.NbPlace <= 0)
                errors.Add("nbplace", "Nombre de places invalide");

            if (VoitureDetailDto.IdCarburant != 4 && VoitureDetailDto.NbCylindres <= 0)
                errors.Add("nbcylindres", "Nombre de cylindres invalide");

            if (VoitureDetailDto.IdBoiteDeVitesse == null || VoitureDetailDto.IdBoiteDeVitesse == 0)
                errors.Add("boitedevitesse", "Veuillez sélectionner une boîte de vitesse");

            if (VoitureDetailDto.IdCategorie == null || VoitureDetailDto.IdCategorie == 0)
                errors.Add("categorie", "Veuillez sélectionner une catégorie");

            if (annonce.Prix == null || annonce.Prix <= 0)
                errors.Add("prix", "Prix invalide");

            if (!selectedCouleurs.Any())
                errors.Add("couleurs", "Veuillez sélectionner au moins une couleur");
            
            if (VoitureDetailDto.IdCarburant != 4 && VoitureDetailDto.CylindrerMoteur <= 0)
                errors.Add("cylindrermoteur", "Cylindrée du moteur invalide");
            
            if (VoitureDetailDto.PositionVolant != null)
                errors.Add("positionvolant", "Veuillez sélectionner la position du volant");

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
        
        public void LoadAddress(AdresseDTO addr)
        {
            selectedAddressId = addr.IdAdresse;
            adresse.Nom = addr.Nom;
            adresse.Numero = addr.Numero;
            adresse.Rue = addr.Rue;
            adresse.CodePostal = addr.CodePostal;
            adresse.LibelleVille = addr.LibelleVille;
    
            if (errors.ContainsKey("nomadresse")) errors.Remove("nomadresse");
            if (errors.ContainsKey("numeroadresse")) errors.Remove("numeroadresse");
            if (errors.ContainsKey("rueadresse")) errors.Remove("rueadresse");
            if (errors.ContainsKey("codepostal")) errors.Remove("codepostal");
            if (errors.ContainsKey("ville")) errors.Remove("ville");
    
            _refreshUI?.Invoke();
        }

        public void ResetAddress()
        {
            selectedAddressId = null;
            adresse = new Adresse();
            _refreshUI?.Invoke();
        }
        
        public void OnPositionVolantChange(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value?.ToString(), out bool value))
            {
                VoitureDetailDto.PositionVolant = value;
                if (errors.ContainsKey("positionvolant"))
                    errors.Remove("positionvolant");
            }
            _refreshUI?.Invoke();
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
                VoitureDetailDTO resultVoitureDetailDto = await _voitureService.CreateAsync(VoitureDetailDto);
                
                foreach (ImageUpload image in imageUpload)
                {
                    image.IdVoiture = resultVoitureDetailDto.IdVoiture;
                    await _postImageService.CreateAsync(image);
                }
                
                foreach (int couleur in selectedCouleurs)
                {
                    APourCouleur aPourCouleur = new APourCouleur()
                    {
                        IdCouleur = couleur,
                        IdVoiture = resultVoitureDetailDto.IdVoiture,
                    };
                    await _aPourCouleurService.CreateAsync(aPourCouleur);
                }
                
                annonce.IdAdresse = resultAdr.IdAdresse;
                annonce.IdVoiture = resultVoitureDetailDto.IdVoiture;
                await _annonceService.CreateAnnonceAsync(annonce);
                _nav.NavigateTo("/");

                VoitureDetailDto = new VoitureDetailDTO();
                adresse = new Adresse();
                annonce = new AnnonceCreateDTO();
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