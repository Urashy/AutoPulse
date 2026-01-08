using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO;
using AutoPulse.Shared.DTO.IA.Data;
using AutoPulse.Shared.DTO.IA.Result;
using BlazorAutoPulse.Helper;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Text.RegularExpressions;
using AutoPulse.Shared.DTO.Immat;
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
        private readonly IService<APourCouleurDTO> _aPourCouleurService;
        private readonly IIAService _iaService;
        private readonly IAutoCompleteService _adresseAutoCompleteService;

        //-------------------------------- Modèles
        public List<ImageUpload> imageUpload;
        public List<AdresseDTO> compteAdresses;
        public string envoieApi;

        private CompteDetailDTO compte;
        public AnnonceCreateDTO annonce;
        public VoitureDetailDTO VoitureDetailDto;
        public AdresseCreateDTO adresse;
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


        // Nouvelles propriétés pour l'autocomplétion
        public string searchQuery { get; set; } = "";
        public List<NominatimResult> addressSuggestions { get; set; } = new();
        public bool showAddressSuggestions { get; set; } = false;
        public bool isSearchingAddress { get; set; } = false;
        
        // Plaque d'immatriculation
        [Parameter]
        public bool IsVisible { get; set; }

        [Parameter]
        public EventCallback<bool> IsVisibleChanged { get; set; }

        [Parameter]
        public EventCallback<VehicleDataDTO> OnApplyData { get; set; }

        public string PlateNumber { get; set; } = "";
        public bool IsLoading { get; set; }
        public bool IsExpanded { get; set; }
        public string ErrorMessage { get; set; } = "";
        public VehicleDataDTO? VehicleData { get; set; }
        
        public bool showImmatModal = false;

        private System.Threading.Timer? _debounceTimer;
        
        // Variables pour le binding des select
        private int _selectedMarqueId;
        public int selectedMarqueId
        {
            get => _selectedMarqueId;
            set
            {
                if (_selectedMarqueId == value) return;

                _selectedMarqueId = value;
                _ = OnMarqueChangedInternal(value);
            }
        }

        private async Task OnMarqueChangedInternal(int marqueId)
        {
            await _vmAll.OnMarqueChanged(marqueId);
            OnMarqueChanged(marqueId);
        }

        private int _selectedModeleId;
        public int selectedModeleId
        {
            get => _selectedModeleId;
            set
            {
                if (_selectedModeleId == value) return;

                _selectedModeleId = value;
                _ = OnModeleChangedInternal(value);
            }
        }
        
        public bool OpenCBModal { get; set; } = false;
        
        public async Task InitializeAsync(Action refreshUI, NavigationManager nav, GetAllViewModel vmAll)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            _vmAll = vmAll;

            try
            {
                compte = await _compteService.GetMe();
                compteAdresses = (await _adresseService.GetAdresseByCompte(compte.IdCompte)).ToList();
            }
            catch (Exception ex)
            {
                _nav.NavigateTo("/connexion");
                Console.WriteLine($"Erreur lors de l'initialisation: {ex.Message}");
            }
        }

        public VenteViewModel(
            ICompteService compteService,
            IAnnonceService annonceService, 
            IVoitureService voitureService, 
            IPostImageService postImageService,
            IAdresseService adresseService,
            IService<APourCouleurDTO> aPourCouleurService,
            IIAService iaService,
            IAutoCompleteService autoCompleteService)
        {
            _compteService = compteService;
            _annonceService = annonceService;
            _voitureService = voitureService;
            _postImageService = postImageService;
            _adresseService = adresseService;
            _aPourCouleurService = aPourCouleurService;
            _iaService = iaService;
            _adresseAutoCompleteService = autoCompleteService;

            imageUpload = new List<ImageUpload>();
            
            annonce = new AnnonceCreateDTO()
            {
                IdCompte = 1,
                IdEtatAnnonce = 1,
                IdMiseEnAvant = -1,
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
                NbAirbag = 4,
                MiseEnCirculation = DateTime.Now,
                InterieurCuire = false,
                CylindrerMoteur = 0,
                PositionVolant = true
            };
            adresse = new AdresseCreateDTO();
            selectedCouleurs = new List<int>();
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

                if (result is ResultatCNN prediction)
                {
                    cnnResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatCNN");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatCNN");
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
                    Category = IADataMapper.TranslateCategory(GetCategorieLibelle()),
                    LeatherInterior = VoitureDetailDto.InterieurCuire ? "Yes" : "No",
                    FuelType = IADataMapper.TranslateFuelType(GetCarburantLibelle()),
                    EngineVolume = (float)VoitureDetailDto.CylindrerMoteur,
                    Mileage = VoitureDetailDto.Kilometrage.ToString(),
                    Cylinders = VoitureDetailDto.NbCylindres > 0 ? (float)VoitureDetailDto.NbCylindres : 4f,
                    GearBoxType = IADataMapper.TranslateGearBoxType(GetBoiteLibelle()),
                    DriveWheels = IADataMapper.TranslateDriveWheels(GetMotriciteLibelle()),
                    Doors = VoitureDetailDto.NbPorte.ToString() ?? "4",
                    Wheel = VoitureDetailDto.PositionVolant ? "Left wheel" : "Right wheel",
                    Color = IADataMapper.TranslateColor(selectedCouleurs.Any() ? GetFirstCouleurLibelle() : "Black"),
                    Airbags = VoitureDetailDto.NbAirbag,
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

                if (result is ResultatAjustement prediction)
                {
                    adjustmentResult = prediction;
                    Console.WriteLine("Cast réussi vers ResultatAjustement");
                }
                else
                {
                    Console.WriteLine($"ERREUR: Type reçu {result?.GetType().Name} au lieu de ResultatAjustement");
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
            CloseAdjustmentResultPopup();
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

        public void OnMarqueChanged(int marqueId)
        {
            VoitureDetailDto.IdMarque = marqueId;
            selectedMarqueId = VoitureDetailDto.IdMarque;
            if (VoitureDetailDto.IdMarque != 0 && errors.ContainsKey("marque"))
                errors.Remove("marque");
            _refreshUI?.Invoke();
        }
        
        public void OnModeleChanged(int modeleId)
        {
            VoitureDetailDto.IdModele = modeleId;
            selectedModeleId =  VoitureDetailDto.IdModele;
            if (VoitureDetailDto.IdModele != 0 && errors.ContainsKey("modele"))
                errors.Remove("modele");
            _refreshUI?.Invoke();
        }

        public void OnCarburantChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdCarburant = int.Parse(e.Value.ToString());
            selectedCarburantId = VoitureDetailDto.IdCarburant;
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
            selectedMotriciteId = VoitureDetailDto.IdMotricite;
            if (VoitureDetailDto.IdMotricite != 0 && errors.ContainsKey("motricite"))
                errors.Remove("motricite");
        }
        
        public void OnBoiteDeVitesseChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdBoiteDeVitesse = int.Parse(e.Value.ToString());
            selectedBoiteId = VoitureDetailDto.IdBoiteDeVitesse;
            if (VoitureDetailDto.IdBoiteDeVitesse != 0 && errors.ContainsKey("boitedevitesse"))
                errors.Remove("boitedevitesse");
        }
        
        public void OnCategorieChange(ChangeEventArgs e)
        {
            VoitureDetailDto.IdCategorie = int.Parse(e.Value.ToString());
            selectedCategorieId = VoitureDetailDto.IdCategorie;
            if (VoitureDetailDto.IdCategorie != 0 && errors.ContainsKey("categorie"))
                errors.Remove("categorie");
        }
        
        public void OnMiseEnAvantChange(ChangeEventArgs e)
        {
            annonce.IdMiseEnAvant = int.Parse(e.Value.ToString());
            if (annonce.IdMiseEnAvant != 0 && errors.ContainsKey("miseEnAvant"))
                errors.Remove("miseEnAvant");
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

            if (string.IsNullOrEmpty(annonce.Libelle))
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
            
            if (VoitureDetailDto.PositionVolant == null)
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

        //---------------------------------------------------------------------------
        // Appelle API d'autocomplétion pour les adresses
        public async Task OnSearchQueryChanged(string value)
        {
            searchQuery = value;

            // Réinitialiser les champs si la recherche est vidée
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                addressSuggestions.Clear();
                showAddressSuggestions = false;
                _refreshUI?.Invoke();
                return;
            }

            // Si moins de 3 caractères, ne rien faire
            if (searchQuery.Length < 3)
            {
                return;
            }

            // Annuler le timer précédent s'il existe
            _debounceTimer?.Dispose();

            // Créer un nouveau timer de 3 secondes
            _debounceTimer = new System.Threading.Timer(async _ =>
            {
                await SearchAddressWithDebounce();
            }, null, 1000, Timeout.Infinite);

            _refreshUI?.Invoke();
        }

        // Nouvelle méthode: Rechercher des adresses après le délai
        private async Task SearchAddressWithDebounce()
        {
            isSearchingAddress = true;
            _refreshUI?.Invoke();

            try
            {
                addressSuggestions = await _adresseAutoCompleteService.SearchAddressAsync(searchQuery);
                showAddressSuggestions = addressSuggestions.Any();
            }
            catch (Exception ex)
            {
                addressSuggestions.Clear();
                showAddressSuggestions = false;
            }
            finally
            {
                isSearchingAddress = false;
                _refreshUI?.Invoke();
            }
        }

        // Nouvelle méthode: Sélectionner une adresse depuis les suggestions
        public void SelectAddressSuggestion(NominatimResult suggestion)
        {
            // Remplir automatiquement les champs
            adresse.Numero = int.TryParse(suggestion.Address.HouseNumber, out int num) ? num : 1;
            adresse.Rue = suggestion.Address.Road ?? "";
            adresse.CodePostal = suggestion.Address.Postcode ?? "";
            adresse.LibelleVille = suggestion.Address.GetCity();

            // Mettre à jour la requête de recherche avec l'adresse complète
            searchQuery = suggestion.DisplayName;

            // Masquer les suggestions
            showAddressSuggestions = false;
            addressSuggestions.Clear();

            // Réinitialiser l'ID d'adresse sélectionnée (nouvelle adresse)
            selectedAddressId = null;

            // Effacer les erreurs si les champs sont maintenant valides
            if (adresse.Numero > 0 && errors.ContainsKey("numeroadresse"))
                errors.Remove("numeroadresse");
            if (!string.IsNullOrWhiteSpace(adresse.Rue) && errors.ContainsKey("rueadresse"))
                errors.Remove("rueadresse");
            if (!string.IsNullOrWhiteSpace(adresse.CodePostal) && errors.ContainsKey("codepostal"))
                errors.Remove("codepostal");
            if (!string.IsNullOrWhiteSpace(adresse.LibelleVille) && errors.ContainsKey("ville"))
                errors.Remove("ville");

            _refreshUI?.Invoke();
        }

        // Nouvelle méthode: Fermer les suggestions
        public void CloseAddressSuggestions()
        {
            showAddressSuggestions = false;
            _refreshUI?.Invoke();
        }

        // Mise à jour de ResetAddress pour aussi réinitialiser la recherche
        public void ResetAddress()
        {
            selectedAddressId = null;
            adresse = new AdresseCreateDTO();
            searchQuery = "";
            addressSuggestions.Clear();
            showAddressSuggestions = false;
            _refreshUI?.Invoke();
        }

        // Mise à jour de LoadAddress pour aussi mettre à jour la recherche
        public void LoadAddress(AdresseDTO addr)
        {
            selectedAddressId = addr.IdAdresse;
            adresse.Nom = addr.Nom;
            adresse.Numero = addr.Numero;
            adresse.Rue = addr.Rue;
            adresse.CodePostal = addr.CodePostal;
            adresse.LibelleVille = addr.LibelleVille;

            // Mettre à jour la recherche avec l'adresse complète
            searchQuery = $"{addr.Numero} {addr.Rue}, {addr.CodePostal} {addr.LibelleVille}";

            // Masquer les suggestions
            showAddressSuggestions = false;
            addressSuggestions.Clear();

            if (errors.ContainsKey("nomadresse")) errors.Remove("nomadresse");
            if (errors.ContainsKey("numeroadresse")) errors.Remove("numeroadresse");
            if (errors.ContainsKey("rueadresse")) errors.Remove("rueadresse");
            if (errors.ContainsKey("codepostal")) errors.Remove("codepostal");
            if (errors.ContainsKey("ville")) errors.Remove("ville");

            _refreshUI?.Invoke();
        }
        
        private async Task OnModeleChangedInternal(int marqueId)
        {
            OnModeleChanged(marqueId);
        }

        public int selectedCarburantId
        {
            get => VoitureDetailDto.IdCarburant;
            set => VoitureDetailDto.IdCarburant = value;
        }

        public int selectedMotriciteId
        {
            get => VoitureDetailDto.IdMotricite;
            set => VoitureDetailDto.IdMotricite = value;
        }

        public int selectedBoiteId
        {
            get => VoitureDetailDto.IdBoiteDeVitesse;
            set => VoitureDetailDto.IdBoiteDeVitesse = value;
        }

        public int selectedCategorieId
        {
            get => VoitureDetailDto.IdCategorie;
            set => VoitureDetailDto.IdCategorie = value;
        }

        public void OpenImmatModal()
        {
            showImmatModal = true;
        }

        public async Task HandleImmatDataApplied(VehicleDataDTO vehicleData)
        {
            if (vehicleData == null) return;

            // Appliquer les données sélectionnées
            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Manufacturer), false)
                && !string.IsNullOrWhiteSpace(vehicleData.Manufacturer))
            {
                var marque = _vmAll.allMarques?.FirstOrDefault(m =>
                    m.LibelleMarque.Equals(vehicleData.Manufacturer, StringComparison.OrdinalIgnoreCase));
                if (marque != null)
                {
                    selectedMarqueId = marque.IdMarque;
                    VoitureDetailDto.IdMarque = marque.IdMarque;
                    await _vmAll.FiltrerModeleParMarquePublic(marque.IdMarque);
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Model), false)
                && !string.IsNullOrWhiteSpace(vehicleData.Model))
            {
                var modele = _vmAll.filteredModeles?.FirstOrDefault(m =>
                    m.LibelleModele.Equals(vehicleData.Model, StringComparison.OrdinalIgnoreCase));
                if (modele != null)
                {
                    selectedModeleId = modele.IdModele;
                    VoitureDetailDto.IdModele = modele.IdModele;
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Year), false)
                && vehicleData.Year.HasValue && vehicleData.Year > 0)
            {
                VoitureDetailDto.Annee = vehicleData.Year.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Category), false)
                && !string.IsNullOrWhiteSpace(vehicleData.Category))
            {
                var categorie = _vmAll.allCategories?.FirstOrDefault(c =>
                    c.LibelleCategorie.Equals(vehicleData.Category, StringComparison.OrdinalIgnoreCase));
                if (categorie != null)
                {
                    selectedCategorieId = categorie.IdCategorie;
                    VoitureDetailDto.IdCategorie = categorie.IdCategorie;
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.FuelType), false)
                && !string.IsNullOrWhiteSpace(vehicleData.FuelType))
            {
                var carburant = _vmAll.allCarburants?.FirstOrDefault(c =>
                    c.LibelleCarburant.Equals(vehicleData.FuelType, StringComparison.OrdinalIgnoreCase));
                if (carburant != null)
                {
                    selectedCarburantId = carburant.IdCarburant;
                    VoitureDetailDto.IdCarburant = carburant.IdCarburant;
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.GearBox), false)
                && !string.IsNullOrWhiteSpace(vehicleData.GearBox))
            {
                var boite = _vmAll.allBoiteDeVitesse?.FirstOrDefault(b =>
                    b.LibelleBoite.Equals(vehicleData.GearBox, StringComparison.OrdinalIgnoreCase));
                if (boite != null)
                {
                    selectedBoiteId = boite.IdBoiteDeVitesse;
                    VoitureDetailDto.IdBoiteDeVitesse = boite.IdBoiteDeVitesse;
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.DriveWheels), false)
                && !string.IsNullOrWhiteSpace(vehicleData.DriveWheels))
            {
                var motricite = _vmAll.allMotricite?.FirstOrDefault(m =>
                    m.LibelleMotricite.Equals(vehicleData.DriveWheels, StringComparison.OrdinalIgnoreCase));
                if (motricite != null)
                {
                    selectedMotriciteId = motricite.IdMotricite;
                    VoitureDetailDto.IdMotricite = motricite.IdMotricite;
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Horsepower), false)
                && vehicleData.Horsepower.HasValue && vehicleData.Horsepower > 0)
            {
                VoitureDetailDto.Puissance = vehicleData.Horsepower.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Torque), false)
                && vehicleData.Torque.HasValue && vehicleData.Torque > 0)
            {
                VoitureDetailDto.Couple = vehicleData.Torque.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Cylinders), false)
                && vehicleData.Cylinders.HasValue && vehicleData.Cylinders > 0)
            {
                VoitureDetailDto.NbCylindres = vehicleData.Cylinders.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.EngineVolume), false)
                && vehicleData.EngineVolume.HasValue && vehicleData.EngineVolume > 0)
            {
                VoitureDetailDto.CylindrerMoteur = vehicleData.EngineVolume.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Doors), false)
                && vehicleData.Doors.HasValue && vehicleData.Doors > 0)
            {
                VoitureDetailDto.NbPorte = vehicleData.Doors.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Seats), false)
                && vehicleData.Seats.HasValue && vehicleData.Seats > 0)
            {
                VoitureDetailDto.NbPlace = vehicleData.Seats.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Color), false)
                && !string.IsNullOrWhiteSpace(vehicleData.Color))
            {
                var couleur = _vmAll.allCouleurs?.FirstOrDefault(c =>
                    c.LibelleCouleur.Equals(vehicleData.Color, StringComparison.OrdinalIgnoreCase));
                if (couleur != null && !selectedCouleurs.Contains(couleur.IdCouleur))
                {
                    selectedCouleurs.Add(couleur.IdCouleur);
                }
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.Airbags), false)
                && vehicleData.Airbags.HasValue && vehicleData.Airbags > 0)
            {
                VoitureDetailDto.NbAirbag = vehicleData.Airbags.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.LeatherInterior), false)
                && vehicleData.LeatherInterior.HasValue)
            {
                VoitureDetailDto.InterieurCuire = vehicleData.LeatherInterior.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.LeftHandDrive), false)
                && vehicleData.LeftHandDrive.HasValue)
            {
                VoitureDetailDto.PositionVolant = vehicleData.LeftHandDrive.Value;
            }

            if (vehicleData.SelectedFields.GetValueOrDefault(nameof(vehicleData.FirstRegistration), false)
                && vehicleData.FirstRegistration.HasValue)
            {
                VoitureDetailDto.MiseEnCirculation = vehicleData.FirstRegistration.Value;
            }

            _refreshUI?.Invoke();
        }

        public async Task CreateAnnonceOrPayMav()
        {
            if (annonce.IdMiseEnAvant == -1)
            {
                return;
            }
            
            if (annonce.IdMiseEnAvant == 1)
            {
                CreateAnnonce();
                return;
            }

            PayMav();
        }

        public async Task PayMav()
        {
            OpenCBModal = true;
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
                AdresseDTO resultAdr = new AdresseDTO();
                if (selectedAddressId == null)
                {
                    adresse.IdPays = 1;
                    adresse.IdCompte = compte.IdCompte;
                    resultAdr = await _adresseService.CreateAdresseAsync(adresse);
                }
                else
                {
                    resultAdr = await _adresseService.GetByIdAsync(selectedAddressId.Value);
                }
                
                VoitureDetailDTO resultVoitureDetailDto = await _voitureService.CreateAsync(VoitureDetailDto);
                
                foreach (ImageUpload image in imageUpload)
                {
                    image.IdVoiture = resultVoitureDetailDto.IdVoiture;
                    Console.WriteLine(image.IdVoiture);
                    await _postImageService.CreateAsync(image);
                }
                
                foreach (int couleur in selectedCouleurs)
                {
                    APourCouleurDTO aPourCouleur = new APourCouleurDTO()
                    {
                        IdCouleur = couleur,
                        IdVoiture = resultVoitureDetailDto.IdVoiture,
                    };
                    await _aPourCouleurService.CreateAsync(aPourCouleur);
                }
                
                annonce.IdAdresse = resultAdr.IdAdresse;
                annonce.IdVoiture = resultVoitureDetailDto.IdVoiture;
                annonce.IdCompte = compte.IdCompte;
                await _annonceService.CreateAnnonceAsync(annonce);
                _nav.NavigateTo("/");

                VoitureDetailDto = new VoitureDetailDTO();
                adresse = new AdresseCreateDTO();
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