using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service;

namespace BlazorAutoPulse.ViewModel;

public class SignalementViewModel
{
    private readonly ISignalementService _signalementService;
    private readonly ITypeSignalementService _typeSignalementService;
    private readonly ICompteService _compteService;
    private readonly NotificationService _notificationService;

    public bool ShowModal { get; set; }
    public bool IsSubmitting { get; set; }
    public string ErrorMessage { get; set; } = "";

    public IEnumerable<TypeSignalementDTO> TypesSignalement { get; set; } = new List<TypeSignalementDTO>();

    public int SelectedTypeSignalement { get; set; }
    public string Description { get; set; } = "";

    public bool IsDescriptionRequired => SelectedTypeSignalement == 10;

    private int? _currentUserId;
    private int? _annonceId;
    private int? _compteSignaleId;
    private Action? _refreshUI;

    public SignalementViewModel(
        ISignalementService signalementService,
        ITypeSignalementService typeSignalementService,
        ICompteService compteService,
        NotificationService notificationService)
    {
        _signalementService = signalementService;
        _typeSignalementService = typeSignalementService;
        _compteService = compteService;
        _notificationService = notificationService;
    }

    public async Task InitializeAsync(Action refreshUI)
    {
        _refreshUI = refreshUI;

        try
        {
            var compte = await _compteService.GetMe();
            _currentUserId = compte?.IdCompte;
        }
        catch
        {
            _currentUserId = null;
        }

        try
        {
            TypesSignalement = await _typeSignalementService.GetAllAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement des types de signalement: {ex.Message}");
            TypesSignalement = new List<TypeSignalementDTO>();
        }
    }

    /// <summary>
    /// Ouvre le modal pour signaler une annonce
    /// </summary>
    public void OpenModalAnnonce(int annonceId)
    {
        if (!_currentUserId.HasValue)
        {
            _notificationService.ShowWarning(
                "Connexion requise",
                "Vous devez être connecté pour signaler une annonce"
            );
            return;
        }

        _annonceId = annonceId;
        _compteSignaleId = null;
        ShowModal = true;
        ResetForm();
        _refreshUI?.Invoke();
    }

    /// <summary>
    /// Ouvre le modal pour signaler un compte
    /// </summary>
    public void OpenModalCompte(int compteId)
    {
        if (!_currentUserId.HasValue)
        {
            _notificationService.ShowWarning(
                "Connexion requise",
                "Vous devez être connecté pour signaler un utilisateur"
            );
            return;
        }

        _compteSignaleId = compteId;
        _annonceId = null;
        ShowModal = true;
        ResetForm();
        _refreshUI?.Invoke();
    }

    public void CloseModal()
    {
        ShowModal = false;
        ResetForm();
        _refreshUI?.Invoke();
    }

    private void ResetForm()
    {
        SelectedTypeSignalement = 0;
        Description = "";
        ErrorMessage = "";
    }

    /// <summary>
    /// Soumet le signalement (annonce ou compte)
    /// </summary>
    public async Task SubmitSignalement()
    {
        if (!_currentUserId.HasValue)
        {
            ErrorMessage = "Vous devez être connecté pour créer un signalement";
            return;
        }

        // Validation
        if (SelectedTypeSignalement == 0)
        {
            ErrorMessage = "Veuillez sélectionner un type de signalement";
            return;
        }

        // Si c'est "Autre" (ID 10), la description est obligatoire
        if (SelectedTypeSignalement == 10 && string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "La description est obligatoire pour le type 'Autre'";
            return;
        }

        IsSubmitting = true;
        ErrorMessage = "";
        _refreshUI?.Invoke();

        try
        {
            var signalement = new SignalementCreateDTO
            {
                IdCompteSignalant = _currentUserId.Value,
                IdAnnonceSignale = _annonceId,
                IdCompteSignale = _compteSignaleId,
                IdTypeSignalement = SelectedTypeSignalement,
                DescriptionSignalement = Description ?? string.Empty
            };

            var result = await _signalementService.CreateAsync(signalement);

            if (result != null)
            {
                var typeMessage = _annonceId.HasValue ? "l'annonce" : "l'utilisateur";
                _notificationService.ShowSuccess(
                    "Signalement envoyé",
                    $"Votre signalement de {typeMessage} a été transmis à nos équipes"
                );
                CloseModal();
            }
            else
            {
                ErrorMessage = "Erreur lors de l'envoi du signalement";
                _notificationService.ShowError(
                    "Erreur",
                    "Une erreur est survenue lors de l'envoi du signalement"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'envoi du signalement: {ex.Message}");
            ErrorMessage = "Une erreur est survenue lors de l'envoi du signalement";
            _notificationService.ShowError(
                "Erreur",
                "Une erreur est survenue lors de l'envoi du signalement"
            );
        }
        finally
        {
            IsSubmitting = false;
            _refreshUI?.Invoke();
        }
    }
}