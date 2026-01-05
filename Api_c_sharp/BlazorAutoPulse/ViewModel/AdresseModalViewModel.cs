using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlazorAutoPulse.Service;

namespace BlazorAutoPulse.ViewModel
{
    public class AdresseModalViewModel
    {
        private readonly IAdresseService _adresseService;
        private readonly IAutoCompleteService _autoCompleteService;
        private readonly ICompteService _compteService;
        private readonly NotificationService _notificationService;

        public AdresseDTO CurrentAdresse { get; set; } = new();
        public string SearchQuery { get; set; } = "";
        public List<NominatimResult> Suggestions { get; set; } = new();
        public bool ShowSuggestions { get; set; } = false;
        public bool IsSearching { get; set; } = false;
        public bool IsSaving { get; set; } = false;
        public string ErrorMessage { get; set; } = "";

        private Dictionary<string, string> _errors = new();
        private System.Threading.Timer? _debounceTimer;
        private Action? _refreshUI;
        private int _idCompte;

        public AdresseModalViewModel(
            IAdresseService adresseService,
            IAutoCompleteService autoCompleteService,
            ICompteService compteService,
            NotificationService notificationService)
        {
            _adresseService = adresseService;
            _autoCompleteService = autoCompleteService;
            _compteService = compteService;
            _notificationService = notificationService;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            try
            {
                var compte = await _compteService.GetMe();
                _idCompte = compte.IdCompte;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur InitializeAsync: {ex.Message}");
            }
        }

        public void ResetForm()
        {
            CurrentAdresse = new AdresseDTO
            {
                IdAdresse = 0,
                IdCompte = _idCompte,
                Nom = "",
                Numero = 0,
                Rue = "",
                CodePostal = "",
                LibelleVille = ""
            };
            SearchQuery = "";
            Suggestions.Clear();
            ShowSuggestions = false;
            ErrorMessage = "";
            _errors.Clear();
            _refreshUI?.Invoke();
        }

        public async Task LoadAdresse(AdresseDTO adresse)
        {
            CurrentAdresse = new AdresseDTO
            {
                IdAdresse = adresse.IdAdresse,
                IdCompte = adresse.IdCompte,
                Nom = adresse.Nom,
                Numero = adresse.Numero,
                Rue = adresse.Rue,
                CodePostal = adresse.CodePostal,
                LibelleVille = adresse.LibelleVille
            };
            SearchQuery = $"{adresse.Numero} {adresse.Rue}, {adresse.CodePostal} {adresse.LibelleVille}";
            _errors.Clear();
            ErrorMessage = "";
            _refreshUI?.Invoke();
        }

        public async Task OnSearchQueryChanged(string value)
        {
            SearchQuery = value;

            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Suggestions.Clear();
                ShowSuggestions = false;
                _refreshUI?.Invoke();
                return;
            }

            if (SearchQuery.Length < 3)
            {
                return;
            }

            _debounceTimer?.Dispose();
            _debounceTimer = new System.Threading.Timer(async _ =>
            {
                await SearchAddressWithDebounce();
            }, null, 1000, Timeout.Infinite);

            _refreshUI?.Invoke();
        }

        private async Task SearchAddressWithDebounce()
        {
            IsSearching = true;
            _refreshUI?.Invoke();

            try
            {
                Suggestions = await _autoCompleteService.SearchAddressAsync(SearchQuery);
                ShowSuggestions = Suggestions.Any();
            }
            catch (Exception ex)
            {
                Suggestions.Clear();
                ShowSuggestions = false;
                Console.WriteLine($"Erreur recherche adresse: {ex.Message}");
            }
            finally
            {
                IsSearching = false;
                _refreshUI?.Invoke();
            }
        }

        public void SelectSuggestion(NominatimResult suggestion)
        {
            CurrentAdresse.Numero = int.TryParse(suggestion.Address.HouseNumber, out int num) ? num : 1;
            CurrentAdresse.Rue = suggestion.Address.Road ?? "";
            CurrentAdresse.CodePostal = suggestion.Address.Postcode ?? "";
            CurrentAdresse.LibelleVille = suggestion.Address.GetCity();

            SearchQuery = suggestion.DisplayName;
            ShowSuggestions = false;
            Suggestions.Clear();

            if (_errors.ContainsKey("numero")) _errors.Remove("numero");
            if (_errors.ContainsKey("rue")) _errors.Remove("rue");
            if (_errors.ContainsKey("codepostal")) _errors.Remove("codepostal");
            if (_errors.ContainsKey("ville")) _errors.Remove("ville");

            _refreshUI?.Invoke();
        }

        public void CloseSuggestions()
        {
            ShowSuggestions = false;
            _refreshUI?.Invoke();
        }

        private bool ValidateForm()
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(CurrentAdresse.Nom))
                _errors.Add("nom", "Le nom de l'adresse est requis");

            if (CurrentAdresse.Numero <= 0)
                _errors.Add("numero", "Le numéro est requis");

            if (string.IsNullOrWhiteSpace(CurrentAdresse.Rue))
                _errors.Add("rue", "La rue est requise");

            if (string.IsNullOrWhiteSpace(CurrentAdresse.CodePostal))
                _errors.Add("codepostal", "Le code postal est requis");

            if (string.IsNullOrWhiteSpace(CurrentAdresse.LibelleVille))
                _errors.Add("ville", "La ville est requise");

            return !_errors.Any();
        }

        public bool HasError(string fieldName)
        {
            return _errors.ContainsKey(fieldName);
        }

        public string GetError(string fieldName)
        {
            return _errors.ContainsKey(fieldName) ? _errors[fieldName] : "";
        }

        public async Task<bool> SaveAdresse(bool isEditMode)
        {
            if (!ValidateForm())
            {
                ErrorMessage = "Veuillez corriger les erreurs avant de continuer";
                _refreshUI?.Invoke();
                return false;
            }

            IsSaving = true;
            ErrorMessage = "";
            _refreshUI?.Invoke();

            try
            {
                if (isEditMode)
                {
                    var adresseToUpdate = new AdresseUpdateDTO()
                    {
                        IdAdresse = CurrentAdresse.IdAdresse,
                        IdCompte = CurrentAdresse.IdCompte,
                        Nom = CurrentAdresse.Nom,
                        Numero = CurrentAdresse.Numero,
                        Rue = CurrentAdresse.Rue,
                        CodePostal = CurrentAdresse.CodePostal,
                        LibelleVille = CurrentAdresse.LibelleVille,
                        IdPays = 1
                    };

                    await _adresseService.UpdateAdresseAsync(CurrentAdresse.IdAdresse, adresseToUpdate);
                    _notificationService.ShowSuccess(
                        "Adresse modifiée",
                        "L'adresse a été modifiée avec succès"
                    );
                }
                else
                {
                    var nouvelleAdresse = new AdresseCreateDTO()
                    {
                        IdCompte = _idCompte,
                        IdPays = 1,
                        Nom = CurrentAdresse.Nom,
                        Numero = CurrentAdresse.Numero,
                        Rue = CurrentAdresse.Rue,
                        CodePostal = CurrentAdresse.CodePostal,
                        LibelleVille = CurrentAdresse.LibelleVille
                    };

                    await _adresseService.CreateAdresseAsync(nouvelleAdresse);
                    _notificationService.ShowSuccess(
                        "Adresse créée",
                        "L'adresse a été créée avec succès"
                    );
                }

                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = "Une erreur est survenue lors de l'enregistrement";
                _notificationService.ShowError(
                    "Erreur",
                    "Impossible d'enregistrer l'adresse"
                );
                Console.WriteLine($"Erreur SaveAdresse: {ex.Message}");
                return false;
            }
            finally
            {
                IsSaving = false;
                _refreshUI?.Invoke();
            }
        }
    }
}