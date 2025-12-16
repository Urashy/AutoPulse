using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class ModifierAnnonceViewModel
    {
        private readonly IAnnonceService _annonceService;
        private readonly IService<VoitureDetailDTO> _voitureService;
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
            IService<VoitureDetailDTO> voitureService,
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

            if (string.IsNullOrWhiteSpace(Annonce?.Libelle))
                errors.Add("titre", "Le titre est requis");

            if (Annonce?.Prix == null || Annonce.Prix <= 0)
                errors.Add("prix", "Le prix doit être supérieur à 0");

            // La description est optionnelle
            
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

        public async Task SaveChanges()
        {
            showErrors = true;
            successMessage = null;

            if (!ValidateForm())
            {
                _refreshUI?.Invoke();
                return;
            }

            if (Annonce == null || !CurrentUserId.HasValue)
            {
                return;
            }

            IsSaving = true;
            _refreshUI?.Invoke();

            try
            {
                var updateDto = new AnnonceUpdateDTO
                {
                    IdAnnonce = Annonce.IdAnnonce,
                    Libelle = Annonce.Libelle,
                    IdCompte = Annonce.IdVendeur,
                    IdEtatAnnonce = 1, // Vous pourriez vouloir récupérer l'état actuel
                    IdAdresse = Annonce.IdAdresse,
                    IdVoiture = Annonce.IdVoiture,
                    IdMiseEnAvant = Annonce.IdMiseEnAvant,
                    DatePublication = Annonce.DatePublication,
                    Prix = Annonce.Prix,
                    Description = Annonce.Description ?? ""
                };

                await _annonceService.UpdateAnnonceAsync(Annonce.IdAnnonce, updateDto);

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