using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BlazorAutoPulse.Service.Interface;

namespace BlazorAutoPulse.ViewModel
{
    public class CompleteProfileViewModel
    {
        private readonly ICompteService _compteService;

        // État du formulaire
        public DateTime dateNaissance { get; set; } = DateTime.Today.AddYears(-18);
        public string? biographie { get; set; }
        public bool isProfessionnel { get; set; } = false;
        public string? numeroSiret { get; set; }
        public string? raisonSociale { get; set; }

        // État UI
        public bool isLoading { get; set; } = false;
        public string? messageErreur { get; set; }
        private Dictionary<string, string> errors = new();
        private int? currentUserId;
        
        
        private Action? _refreshUI;
        private NavigationManager? _navigation;

        public CompleteProfileViewModel(ICompteService compteService)
        {
            _compteService = compteService;
        }

        public async Task InitializeAsync(Action stateHasChanged, NavigationManager navigation)
        {
            _refreshUI = stateHasChanged;
            _navigation = navigation;

            // Récupérer l'utilisateur connecté
            var compte = await _compteService.GetMe();
            if (compte != null)
            {
                currentUserId = compte.IdCompte;
                dateNaissance = compte.DateNaissance;
                biographie = compte.Biographie;
                isProfessionnel = compte.IdTypeCompte == 2;
                numeroSiret = compte.NumeroSiret;
                raisonSociale = compte.RaisonSociale;
            }
        }

        public bool HasError(string field) => errors.ContainsKey(field);

        public string GetError(string field) => errors.ContainsKey(field) ? errors[field] : "";

        private bool ValidateForm()
        {
            errors.Clear();
            messageErreur = null;

            // Validation date de naissance
            if (dateNaissance == default || dateNaissance.Year == 2000)
            {
                errors["datenaissance"] = "Veuillez renseigner votre date de naissance";
            }
            else if (dateNaissance > DateTime.Today.AddYears(-18))
            {
                errors["datenaissance"] = "Vous devez avoir au moins 18 ans";
            }
            else if (dateNaissance < DateTime.Today.AddYears(-120))
            {
                errors["datenaissance"] = "Date de naissance invalide";
            }

            // Validation champs professionnels
            if (isProfessionnel)
            {
                if (string.IsNullOrWhiteSpace(numeroSiret))
                {
                    errors["siret"] = "Le numéro SIRET est requis pour un professionnel";
                }
                else if (!Regex.IsMatch(numeroSiret.Trim(), @"^\d{14}$"))
                {
                    errors["siret"] = "Le SIRET doit contenir exactement 14 chiffres";
                }

                if (string.IsNullOrWhiteSpace(raisonSociale))
                {
                    errors["raisonsociale"] = "La raison sociale est requise pour un professionnel";
                }
            }

            if (errors.Any())
            {
                messageErreur = "Veuillez corriger les erreurs dans le formulaire";
                return false;
            }

            return true;
        }

        public async Task CompleteProfile()
        {
            if (!ValidateForm())
            {
                _refreshUI?.Invoke();
                return;
            }

            isLoading = true;
            _refreshUI?.Invoke();

            try
            {
                if (currentUserId == null)
                {
                    messageErreur = "Session expirée, veuillez vous reconnecter";
                    _navigation?.NavigateTo("/connexion");
                    return;
                }

                // Récupérer le compte complet pour la mise à jour
                var compte = await _compteService.GetByIdAsync(currentUserId.Value);
                
                if (compte == null)
                {
                    messageErreur = "Erreur lors de la récupération du compte";
                    return;
                }

                // Créer le DTO de mise à jour
                Compte updateDto = new Compte()
                {
                    IdCompte = compte.IdCompte,
                    Pseudo = compte.Pseudo,
                    Nom = compte.Nom,
                    Prenom = compte.Prenom,
                    Email = compte.Email,
                    DateNaissance = DateTime.SpecifyKind(dateNaissance, DateTimeKind.Utc),
                    Biographie = string.IsNullOrWhiteSpace(biographie) ? null : biographie.Trim(),
                    IdTypeCompte = isProfessionnel ? 1 : 2,
                    NumeroSiret = isProfessionnel ? numeroSiret?.Trim() : null,
                    RaisonSociale = isProfessionnel ? raisonSociale?.Trim() : null
                };

                await _compteService.UpdateAsync(currentUserId.Value, updateDto);

                _navigation?.NavigateTo("/compte");
            }
            catch (Exception ex)
            {
                messageErreur = $"Erreur : {ex.Message}";
            }
            finally
            {
                isLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public void SkipForNow()
        {
            _navigation?.NavigateTo("/compte");
        }
    }
}