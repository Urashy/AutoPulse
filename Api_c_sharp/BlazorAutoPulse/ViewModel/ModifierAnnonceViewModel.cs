using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class ModifierAnnonceViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IVoitureService _voitureService;
        private readonly ICompteService _compteService;
        private readonly NotificationService _notificationService;

        public AnnonceDetailDTO? Annonce { get; private set; }
        public VoitureDetailDTO? Voiture { get; private set; }
        public bool IsLoading { get; private set; } = true;
        public bool IsSaving { get; private set; } = false;
        public int? CurrentUserId { get; private set; }
        
        public Dictionary<string, string> errors { get; set; } = new();
        public bool showErrors { get; set; } = false;
        public string? successMessage { get; set; } = null;

        private Action? _refreshUI;
        private NavigationManager? _nav;

        public ModifierAnnonceViewModel(
            IAnnonceService annonceService,
            ICompteService compteService,
            IVoitureService voitureService,
            NotificationService notificationService)
        {
            _annonceService = annonceService;
            _compteService = compteService;
            _voitureService = voitureService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(int idAnnonce, Action refreshUI, NavigationManager nav)
        {
            _refreshUI = refreshUI;
            _nav = nav;
            IsLoading = true;
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
                Voiture = await _voitureService.GetByIdAsync(Annonce.IdVoiture);
                Console.WriteLine(Annonce.IdVoiture);

                // Vérifier que l'utilisateur est bien le propriétaire
                if (Annonce == null || Annonce.IdVendeur != CurrentUserId)
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
                Annonce = null;
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

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
        
        public void OnMiseEnAvantChange(ChangeEventArgs e)
        {
            Annonce.IdMiseEnAvant = int.Parse(e.Value.ToString());
            if (Annonce.IdMiseEnAvant != 0 && errors.ContainsKey("miseEnAvant"))
                errors.Remove("miseEnAvant");
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