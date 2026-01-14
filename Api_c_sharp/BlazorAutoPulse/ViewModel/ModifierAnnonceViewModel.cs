using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Helper;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorAutoPulse.ViewModel
{
    public class ModifierAnnonceViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IVoitureService _voitureService;
        private readonly ICompteService _compteService;
        private readonly IPostImageService _postImageService;
        private readonly IImageService _imageService;
        private readonly NotificationService _notificationService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public VoitureDetailDTO? Voiture { get; private set; }
        public bool IsLoading { get; private set; } = true;
        public bool IsSaving { get; private set; } = false;
        public int? CurrentUserId { get; private set; }
        
        public Dictionary<string, string> errors { get; set; } = new();
        public bool showErrors { get; set; } = false;
        public string? successMessage { get; set; } = null;
        
        public bool showPaiementMavModal { get; set; } = false;
        public int IdCbUse { get; set; } = 0;
        
        // ============================================================================
        // PROPRIÉTÉS POUR LE CARROUSEL D'IMAGES
        // ============================================================================
        public List<ImageUpload> imageUpload { get; set; } = new();
        public List<string> nomPhotos { get; set; } = new();
        public List<int> ExistingImageIds { get; private set; } = new();
        public List<int> imagesToDelete { get; set; } = new();
        public bool IsLoadingImages { get; private set; } = false;
        
        public int CurrentImageIndex { get; set; } = 0;
        public bool CanGoPrevious => CurrentImageIndex > 0;
        public bool CanGoNext => CurrentImageIndex < TotalImageCount - 1;
        private Dictionary<int, string> _imageCache = new Dictionary<int, string>();
        
        public int TotalImageCount => ExistingImageIds.Count + imageUpload.Count;
        
        private int _selectedMavId;
        public int selectedMavId
        {
            get => _selectedMavId;
            set
            {
                if (_selectedMavId == value) return;

                _selectedMavId = value;
                _ = OnMavChangedInternal(value);
            }
        }
        
        private async Task OnMavChangedInternal(int marqueId)
        {
            OnMiseEnAvantChange(marqueId);
        }

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public ModifierAnnonceViewModel(
            IAnnonceService annonceService,
            ICompteService compteService,
            IVoitureService voitureService,
            IPostImageService postImageService,
            IImageService imageService,
            NotificationService notificationService)
        {
            _annonceService = annonceService;
            _compteService = compteService;
            _voitureService = voitureService;
            _postImageService = postImageService;
            _imageService = imageService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            IsLoading = true;
            successMessage = null;
            
            // ✅ Initialiser les collections pour éviter les NullReferenceException
            ExistingImageIds = new List<int>();
            imageUpload = new List<ImageUpload>();
            nomPhotos = new List<string>();
            imagesToDelete = new List<int>();
            errors = new Dictionary<string, string>();
            
            _refreshUI?.Invoke();
            
            try
            {
                // Vérifier que l'utilisateur est connecté
                try
                {
                    CompteDetailDTO user = await _compteService.GetMe();
                    CurrentUserId = user.IdCompte;
                }
                catch
                {
                    CurrentUserId = null;
                    _nav?.NavigateTo("/connexion");
                    return;
                }
                // Charger l'annonce
                Annonce = await _annonceService.GetAnnonceDetailById(idAnnonce);

                if (Annonce == null)
                {
                    Console.WriteLine("❌ Annonce introuvable");
                    _nav?.NavigateTo("/");
                    return;
                }

                selectedMavId = Annonce.IdMiseEnAvant;
                
                Voiture = await _voitureService.GetByIdAsync(Annonce.IdVoiture);

                if (Voiture == null)
                {
                    Console.WriteLine("❌ Voiture introuvable");
                    _nav?.NavigateTo("/");
                    return;
                }

                await LoadAllImages();
                
                Console.WriteLine($"Annonce chargée: {Annonce.IdVoiture}, {ExistingImageIds.Count} images");

                if (Annonce.IdVendeur != CurrentUserId)
                {
                    _notificationService.ShowError(
                        "Accès refusé",
                        "Vous n'avez pas les droits pour modifier cette annonce"
                    );
                    _nav?.NavigateTo("/");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'annonce: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Annonce = null;
                Voiture = null;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        // ============================================================================
        // MÉTHODES CARROUSEL D'IMAGES (approche IImageService)
        // ============================================================================
        
        // ✅ Charger tous les IDs d'images (comme AnnonceDetailViewModel)
        private async Task LoadAllImages()
        {
            if (Voiture == null)
            {
                Console.WriteLine("⚠️ Voiture est null, impossible de charger les images");
                ExistingImageIds = new List<int>();
                return;
            }

            IsLoadingImages = true;
            _refreshUI?.Invoke();

            try
            {
                ExistingImageIds = await _imageService.GetAllImageIdsByVoitureId(Voiture.IdVoiture);

                if (ExistingImageIds == null)
                {
                    Console.WriteLine("⚠️ GetAllImageIdsByVoitureId a retourné null");
                    ExistingImageIds = new List<int>();
                }
                else if (!ExistingImageIds.Any())
                {
                    Console.WriteLine("⚠️ Aucune image trouvée pour cette voiture");
                }
                else
                {
                    Console.WriteLine($"✅ {ExistingImageIds.Count} image(s) chargée(s)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur lors du chargement des images: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ExistingImageIds = new List<int>();
            }
            finally
            {
                IsLoadingImages = false;
                _refreshUI?.Invoke();
            }
        }
        
        public async Task UploadImage(InputFileChangeEventArgs e)
        {
            var files = e.GetMultipleFiles(10);

            try
            {
                foreach (var file in files)
                {
                    nomPhotos.Add(file.Name);

                    try
                    {
                        const long maxFileSize = 10 * 1024 * 1024;

                        using var memoryStream = new MemoryStream();
                        using var stream = file.OpenReadStream(maxFileSize);
                        await stream.CopyToAsync(memoryStream);

                        var imageBytes = memoryStream.ToArray();
                        var base64 = Convert.ToBase64String(imageBytes);

                        ImageUpload image = new ImageUpload
                        {
                            File = file,
                            ImageBytes = imageBytes
                        };
                        imageUpload.Add(image);

                        // Cache pour les nouvelles images uploadées
                        var imageIndex = ExistingImageIds.Count + imageUpload.Count - 1;
                        _imageCache[imageIndex] = $"data:{file.ContentType};base64,{base64}";

                        Console.WriteLine($"✅ Image {imageIndex} chargée : {file.Name} ({imageBytes.Length} bytes)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur lors du chargement de {file.Name}: {ex.Message}");
                    }

                    _refreshUI?.Invoke();
                }
                
                // Supprimer l'erreur photos si on a maintenant des images
                if (TotalImageCount > 0 && errors.ContainsKey("photos"))
                {
                    errors.Remove("photos");
                }
            }
            catch
            {
            }
        }
        
        public void NextImage()
        {
            if (CanGoNext)
            {
                CurrentImageIndex++;
                _refreshUI?.Invoke();
            }
        }
        
        public void PreviousImage()
        {
            if (CanGoPrevious)
            {
                CurrentImageIndex--;
                _refreshUI?.Invoke();
            }
        }
        
        public void SelectImage(int index)
        {
            if (index >= 0 && index < TotalImageCount)
            {
                CurrentImageIndex = index;
                _refreshUI?.Invoke();
            }
        }
        
        // ✅ Récupérer l'URL de l'image courante (images existantes via IImageService)
        public string GetCurrentImageUrl()
        {
            if (TotalImageCount == 0 || CurrentImageIndex < 0 || CurrentImageIndex >= TotalImageCount)
                return string.Empty;

            try
            {
                // Si c'est une image existante, utiliser IImageService
                if (CurrentImageIndex < ExistingImageIds.Count)
                {
                    var imageId = ExistingImageIds[CurrentImageIndex];
                    var imageUrl = _imageService.GetImage(imageId);
                    return imageUrl ?? string.Empty;
                }
            
                // Si c'est une nouvelle image uploadée, utiliser le cache
                if (_imageCache.ContainsKey(CurrentImageIndex))
                {
                    return _imageCache[CurrentImageIndex];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetCurrentImageUrl: {ex.Message}");
            }

            return string.Empty;
        }
        
        // ✅ Récupérer l'URL d'une miniature
        public string GetThumbnailUrl(int index)
        {
            if (index < 0 || index >= TotalImageCount)
                return string.Empty;

            try
            {
                // Si c'est une image existante, utiliser IImageService
                if (index < ExistingImageIds.Count)
                {
                    var imageId = ExistingImageIds[index];
                    var imageUrl = _imageService.GetImage(imageId);
                    return imageUrl ?? string.Empty;
                }

                // Si c'est une nouvelle image uploadée, utiliser le cache
                if (_imageCache.ContainsKey(index))
                {
                    return _imageCache[index];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur GetThumbnailUrl pour index {index}: {ex.Message}");
            }

            return string.Empty;
        }
        
        public void RemoveImage(int index)
        {
            if (index >= 0 && index < TotalImageCount)
            {
                // Si c'est une image existante
                if (index < ExistingImageIds.Count)
                {
                    // Marquer l'image pour suppression (vous pourrez l'implémenter côté serveur)
                    var imageIdToDelete = ExistingImageIds[index];
                    imagesToDelete.Add(imageIdToDelete);
                    ExistingImageIds.RemoveAt(index);
                    Console.WriteLine($"🗑️ Image {imageIdToDelete} marquée pour suppression");
                }
                else
                {
                    // Si c'est une nouvelle image uploadée
                    var newImageIndex = index - ExistingImageIds.Count;
                    if (newImageIndex >= 0 && newImageIndex < imageUpload.Count)
                    {
                        imageUpload.RemoveAt(newImageIndex);
                        nomPhotos.RemoveAt(newImageIndex);
                    }
                }
                
                // Réorganiser le cache
                var newCache = new Dictionary<int, string>();
                for (int i = 0; i < TotalImageCount; i++)
                {
                    if (i < index && _imageCache.ContainsKey(i))
                    {
                        newCache[i] = _imageCache[i];
                    }
                    else if (i >= index && _imageCache.ContainsKey(i + 1))
                    {
                        newCache[i] = _imageCache[i + 1];
                    }
                }
                _imageCache = newCache;
        
                if (CurrentImageIndex >= TotalImageCount && TotalImageCount > 0)
                {
                    CurrentImageIndex = TotalImageCount - 1;
                }
                else if (TotalImageCount == 0)
                {
                    CurrentImageIndex = 0;
                }
        
                if (TotalImageCount == 0 && !errors.ContainsKey("photos"))
                {
                    errors.Add("photos", "Au moins une photo est requise");
                }
        
                _refreshUI?.Invoke();
            }
        }
        
        public void RemoveCurrentImage()
        {
            RemoveImage(CurrentImageIndex);
        }
        
        public bool IsExistingImage(int index)
        {
            return index < ExistingImageIds.Count;
        }

        // ============================================================================
        // MÉTHODES EXISTANTES (validation, sauvegarde, etc.)
        // ============================================================================

        private bool ValidateForm()
        {
            errors.Clear();

            // Validation de l'annonce
            if (string.IsNullOrWhiteSpace(Annonce?.Libelle))
                errors.Add("titre", "Le titre est requis");
            else if (Annonce.Libelle.Length > 200)
                errors.Add("titre", "Le titre ne peut pas dépasser 200 caractères");

            if (Annonce?.Prix == null || Annonce.Prix <= 0)
                errors.Add("prix", "Le prix doit être supérieur à 0");
            
            // Validation des images
            if (TotalImageCount == 0)
                errors.Add("photos", "Au moins une photo est requise");

            // Validation de la voiture
            if (Voiture != null)
            {
                if (Voiture.Kilometrage < 0)
                    errors.Add("kilometrage", "Le kilométrage ne peut pas être négatif");

                if (Voiture.Annee < 1900 || Voiture.Annee > DateTime.Now.Year)
                    errors.Add("annee", $"L'année doit être entre 1900 et {DateTime.Now.Year}");

                if (Voiture.MiseEnCirculation > DateTime.Now)
                    errors.Add("miseEnCirculation", "La date de mise en circulation ne peut pas être dans le futur");

                if (Voiture.Puissance <= 0)
                    errors.Add("puissance", "La puissance doit être supérieure à 0");

                if (Voiture.Couple < 0)
                    errors.Add("couple", "Le couple ne peut pas être négatif");

                if (Voiture.NbCylindres < 1 || Voiture.NbCylindres > 16)
                    errors.Add("nbCylindres", "Le nombre de cylindres doit être entre 1 et 16");

                if (Voiture.CylindrerMoteur <= 0)
                    errors.Add("cylindrerMoteur", "La cylindrée doit être supérieure à 0");

                if (Voiture.NbPlace < 1 || Voiture.NbPlace > 9)
                    errors.Add("nbPlaces", "Le nombre de places doit être entre 1 et 9");

                if (Voiture.NbPorte < 2 || Voiture.NbPorte > 7)
                    errors.Add("nbPortes", "Le nombre de portes doit être entre 2 et 7");
            }
            
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
        
        public void OnMiseEnAvantChange(int id)
        {
            Annonce.IdMiseEnAvant = id;
            if (Annonce.IdMiseEnAvant != 0 && errors.ContainsKey("miseEnAvant"))
                errors.Remove("miseEnAvant");
        }

        public async Task SaveChangeOrUpdateCB()
        {
            if (Annonce.IdMiseEnAvant == 1)
            {
                await SaveChanges();
                return;
            }
            
            showPaiementMavModal = true;
            _refreshUI?.Invoke();
        }
        
        public void SetIdCb(int id)
        {
            IdCbUse = id;
        }
        
        public async Task OnPaymentSuccess()
        {
            showPaiementMavModal = false;
            
            await SaveChanges();
        }

        public async Task SaveChanges()
        {
            showErrors = true;
            successMessage = null;

            if (!ValidateForm())
            {
                errors.Add("general", "Veuillez corriger les erreurs dans le formulaire");
                _refreshUI?.Invoke();
                return;
            }

            if (Annonce == null || Voiture == null || !CurrentUserId.HasValue)
            {
                Console.WriteLine("❌ SaveChanges: Annonce, Voiture ou CurrentUserId est null");
                errors.Add("general", "Erreur: données manquantes");
                _refreshUI?.Invoke();
                return;
            }

            IsSaving = true;
            _refreshUI?.Invoke();

            try
            {
                // Mise à jour de l'annonce
                var updateAnnonceDto = new AnnonceUpdateDTO
                {
                    IdAnnonce = Annonce.IdAnnonce,
                    Libelle = Annonce.Libelle,
                    IdCompte = Annonce.IdVendeur,
                    IdEtatAnnonce = 1,
                    IdAdresse = Annonce.IdAdresse,
                    IdVoiture = Annonce.IdVoiture,
                    IdMiseEnAvant = Annonce.IdMiseEnAvant,
                    DatePublication = Annonce.DatePublication,
                    Prix = Annonce.Prix,
                    Description = Annonce.Description ?? "",
                    IdCB = IdCbUse
                };

                await _annonceService.UpdateAnnonceAsync(Annonce.IdAnnonce, updateAnnonceDto);

                // Mise à jour de la voiture
                var updateVoitureDto = new VoitureUpdateDTO
                {
                    IdVoiture = Voiture.IdVoiture,
                    IdMarque = Voiture.IdMarque,
                    IdModele = Voiture.IdModele,
                    IdMotricite = Voiture.IdMotricite,
                    IdCarburant = Voiture.IdCarburant,
                    IdBoiteDeVitesse = Voiture.IdBoiteDeVitesse,
                    IdCategorie = Voiture.IdCategorie,
                    NbPlace = Voiture.NbPlace,
                    NbPorte = Voiture.NbPorte,
                    Kilometrage = Voiture.Kilometrage,
                    Annee = Voiture.Annee,
                    Puissance = Voiture.Puissance,
                    Couple = Voiture.Couple,
                    NbCylindres = Voiture.NbCylindres,
                    InterieurCuire = Voiture.InterieurCuire,
                    CylindrerMoteur = Voiture.CylindrerMoteur,
                    PositionVolant = Voiture.PositionVolant,
                    MiseEnCirculation = Voiture.MiseEnCirculation,
                    IdModeleBlender = Voiture.IdModeleBlender
                };

                await _voitureService.UpdateVoitureAsync(Voiture.IdVoiture, updateVoitureDto);
                
                // Supprimer les images marquées pour suppression
                if (imagesToDelete != null && imagesToDelete.Any())
                {
                    Console.WriteLine($"🗑️ Suppression de {imagesToDelete.Count} image(s)");
                    foreach (int imageId in imagesToDelete)
                    {
                        try
                        {
                            await _imageService.DeleteAsync(imageId);
                            Console.WriteLine($"🗑️ Image {imageId} supprimée");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Erreur suppression image {imageId}: {ex.Message}");
                        }
                    }
                }
                
                // Ajouter les nouvelles images
                if (imageUpload != null && imageUpload.Any())
                {
                    Console.WriteLine($"📤 Upload de {imageUpload.Count} nouvelle(s) image(s)");
                    foreach (ImageUpload image in imageUpload)
                    {
                        try
                        {
                            image.IdVoiture = Voiture.IdVoiture;
                            await _postImageService.CreateAsync(image);
                            Console.WriteLine($"✅ Image uploadée: {image.File?.Name}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Erreur upload image: {ex.Message}");
                        }
                    }
                }

                _notificationService.ShowSuccess(
                    "Modification réussie",
                    "Votre annonce a été mise à jour avec succès"
                );

                successMessage = "Modifications enregistrées avec succès !";
                
                // Rediriger vers la page de l'annonce après 2 secondes
                _ = Task.Run(async () =>
                {
                    await Task.Delay(2000);
                    _nav?.NavigateTo($"/annonce/{Annonce.IdAnnonce}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                errors.Add("general", "Une erreur est survenue lors de l'enregistrement. Veuillez réessayer.");
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible de mettre à jour l'annonce"
                );
            }
            finally
            {
                IsSaving = false;
                _refreshUI?.Invoke();
            }
        }
    }
}