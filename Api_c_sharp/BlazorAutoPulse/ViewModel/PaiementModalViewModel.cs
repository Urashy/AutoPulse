using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.Components;

public class PaiementModalViewModel
{
    private readonly ICarteBancaireService _carteService;
    private readonly IService<MiseEnAvantDTO> _miseEnAvantService;
    private readonly ICommandeService _commandeService;
    private readonly ICompteService _compteService;

    public string title { get; set; } = "";
    public double prix { get; set; } = 0;

    public bool IsVisible { get; private set; }
    public bool IsLoadingCards { get; private set; }
    public bool ShowCarteBancaireModal { get; private set; }

    public int? SelectedCarteId { get; private set; }
    public List<CarteBancaireDTO> CartesBancaires { get; private set; } = [];
    public MiseEnAvantDTO MiseEnAvantDTO { get; private set; }
    public CommandeDTO CommandeDto { get; private set; }

    public bool CanPay => SelectedCarteId.HasValue;

    private EventCallback<bool> _isVisibleChanged;
    private EventCallback<int> _idCB;
    private EventCallback _onPaymentSuccess;
    private Action _stateHasChanged = () => { };
    
    public PaiementModalViewModel(
        ICarteBancaireService carteService,
        IService<MiseEnAvantDTO> miseEnAvantService,
        ICommandeService commandeService,
        ICompteService compteService)
    {
        _carteService = carteService;
        _miseEnAvantService = miseEnAvantService;
        _commandeService = commandeService;
        _compteService = compteService;
        
        MiseEnAvantDTO = new MiseEnAvantDTO();
    }

    public async Task InitializeAsync(
        bool isVisible,
        EventCallback<bool> isVisibleChanged,
        EventCallback<int> idCB,
        int Id,
        string type,
        EventCallback onPaymentSuccess,
        Action stateHasChanged)
    {
        IsVisible = isVisible;
        _isVisibleChanged = isVisibleChanged;
        _idCB = idCB;
        _onPaymentSuccess = onPaymentSuccess;
        _stateHasChanged = stateHasChanged;

        if (type == "vente")
        {
            if (Id != -1)
            {
                MiseEnAvantDTO = await _miseEnAvantService.GetByIdAsync(Id);
                title = $"Mise en avant: {MiseEnAvantDTO.LibelleMiseEnAvant}";
                prix = MiseEnAvantDTO.PrixSemaine;
            }
        }
        else if (type == "commande")
        {
            if (Id != -1)
            {
                CommandeDto = await _commandeService.GetByIdAsync(Id);
                title = $"Numéro de commande: {CommandeDto.IdCommande} / Annonce : {CommandeDto.LibelleAnnonce}";
                prix = CommandeDto.Montant.HasValue ? (double)CommandeDto.Montant.Value : 0;
            }
        }

        if (IsVisible && !CartesBancaires.Any())
            await LoadCartes();
    }

    public async Task LoadCartes()
    {
        IsLoadingCards = true;
        _stateHasChanged();

        try
        {
            var compte = await _compteService.GetMe();
            CartesBancaires = (await _carteService
                .GetCarteBancaireByCompte(compte.IdCompte)).ToList();
        }
        catch
        {
            CartesBancaires = [];
        }
        finally
        {
            IsLoadingCards = false;
            _stateHasChanged();
        }
    }

    public bool IsCarteSelected(int id) => SelectedCarteId == id;

    public async Task SelectCarte(int id)
    {
        SelectedCarteId = id;
        await _idCB.InvokeAsync(id);
        _stateHasChanged();
    }

    public async Task CloseModal()
    {
        SelectedCarteId = null;
        await _isVisibleChanged.InvokeAsync(false);
    }

    public void OpenCarteBancaireModal()
    {
        ShowCarteBancaireModal = true;
        _stateHasChanged();
    }

    public Task ChangeShowCarteModal(bool visible)
    {
        ShowCarteBancaireModal = visible;
        _stateHasChanged();
        return Task.CompletedTask;
    }

    public async Task HandleCarteSaved()
    {
        ShowCarteBancaireModal = false;
        await LoadCartes();
    }

    public async Task ProcessPayment()
    {
        if (!CanPay) return;
        await _onPaymentSuccess.InvokeAsync();
        await CloseModal();
    }
}