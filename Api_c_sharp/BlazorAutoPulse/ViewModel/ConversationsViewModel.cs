using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorAutoPulse.ViewModel;

public class ConversationViewModel : IDisposable
{
    private readonly ConversationStateService _conversationState;
    private readonly ISignalRService _signalR;
    private readonly IMessageService _messageService;
    private readonly IPieceJointeService _pieceJointeService;
    private readonly IBloqueService _bloqueService;
    private readonly IJSRuntime _jsRuntime;
    private readonly IOffreService _offreService;
    private readonly IAnnonceService _annonceService;
    private readonly NotificationService _notificationService; 
    private readonly ICommandeService _commandeService;
    private readonly IConversationService _conversationService;

    public List<MessageDTO> Messages { get; private set; } = new();
    public CommandeDTO? CommandeEnCours { get; private set; }
    public ConversationListDTO? SelectedConversation { get; private set; }
    
    private string _newMessage = "";
    public string NewMessage 
    { 
        get => _newMessage;
        set => _newMessage = value;
    }
    
    public bool IsTyping { get; private set; } = false;
    public ElementReference MessagesContainer;

    public List<IBrowserFile> SelectedFiles { get; set; } = new();
    public bool IsUploadingFiles { get; set; } = false;
    public bool IsLoadingMessages { get; private set; } = false;

    private System.Threading.Timer? _typingTimer;
    private bool _typingNotified = false;
    
    public event Action? _refreshUI;

    public List<ConversationListDTO> Conversations => _conversationState.Conversations;
    public int CurrentUserId => _conversationState.CurrentUserId;
    private bool _isLoading = false;
    public bool IsLoading 
    { 
        get => _isLoading;
        private set => _isLoading = value;
    }
    public Dictionary<int, string> ImageSources => _conversationState.ImageSources;
    
    public bool EstBloquer { get; set; }
    public string BlocageType { get; set; }

    public bool ShowOffreMode { get; set; } = false;
    public decimal OffreAmount { get; set; } = 0;
    public string OffreError { get; set; } = "";
    public int? AnnonceIdForOffre { get; set; }
    public decimal? AnnoncePrixMax { get; set; }

    public string OffreInfoMessage { get; private set; } = "";
    public string OffreInfoClass { get; private set; } = "";
    
    public event Func<Task>? OnScrollRequested;

    // ========== NOUVEAU : Filtrage par annonce ==========
    public List<AnnonceDTO> MesAnnonces { get; private set; } = new();
    public int SelectedAnnonceFilter { get; private set; } = 0;
    public bool IsLoadingAnnonces { get; private set; } = false;
    public List<ConversationListDTO> ConversationsFiltered { get; private set; } = new();

    public ConversationViewModel(
        ConversationStateService conversationState,
        ISignalRService signalR,
        IMessageService msgService,
        IPieceJointeService pieceJointeService,
        IBloqueService bloqueService,
        IJSRuntime jsRuntime,
        IOffreService offreService,
        IAnnonceService annonceService,
        NotificationService notificationService,
        ICommandeService commandeService,
        IConversationService conversationService)
    {
        _conversationState = conversationState;
        _signalR = signalR;
        _messageService = msgService;
        _pieceJointeService = pieceJointeService;
        _bloqueService = bloqueService;
        _jsRuntime = jsRuntime;
        _offreService = offreService;
        _annonceService = annonceService;
        _notificationService = notificationService;
        _commandeService = commandeService;
        _conversationService = conversationService;

        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
        _conversationState.OnStateChanged += HandleGlobalStateChanged;
        _signalR.OnOffreStatusChanged += HandleOffreStatusChanged;
        _signalR.OnMessageWithOffreReceived += HandleMessageWithOffreReceived;
    }

    public async Task InitializeAsync()
    {
        await _conversationState.InitializeAsync();
        await LoadMesAnnonces();
        await LoadConversations(0); // Charge toutes les conversations au départ
    }

    // ========== NOUVEAU : Chargement des conversations avec filtre ==========
    private async Task LoadConversations(int idAnnonce)
    {
        IsLoading = true;
        NotifyStateChanged();

        try
        {
            var conversations = await _conversationService.GetConversationsByCompteID(CurrentUserId, idAnnonce);
            ConversationsFiltered = conversations.ToList();
            
            // Rejoindre les conversations SignalR et charger les images
            foreach (var conv in ConversationsFiltered)
            {
                if (!_conversationState.Conversations.Any(c => c.IdConversation == conv.IdConversation))
                {
                    await _signalR.JoinConversation(conv.IdConversation);
                }
                
                if (!ImageSources.ContainsKey(conv.IdParticipant))
                {
                    await GetImageProfil(conv.IdParticipant);
                }
            }
            
            Console.WriteLine($"✅ {ConversationsFiltered.Count} conversations chargées (filtre annonce: {idAnnonce})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement conversations: {ex.Message}");
            ConversationsFiltered = new List<ConversationListDTO>();
        }
        finally
        {
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    private async Task GetImageProfil(int idCompte)
    {
        if (ImageSources.ContainsKey(idCompte))
            return;

        try
        {
            var img = await _conversationState.GetImageProfilAsync(idCompte);
            if (!string.IsNullOrEmpty(img))
            {
                ImageSources[idCompte] = img;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement image {idCompte}: {ex.Message}");
        }
    }

    // ========== NOUVEAU : Chargement des annonces de l'utilisateur ==========
    private async Task LoadMesAnnonces()
    {
        IsLoadingAnnonces = true;
        NotifyStateChanged();

        try
        {
            var annonces = await _annonceService.GetByCompteID(CurrentUserId);
            MesAnnonces = annonces.ToList();
            Console.WriteLine($"✅ {MesAnnonces.Count} annonces chargées pour filtrage");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement annonces: {ex.Message}");
        }
        finally
        {
            IsLoadingAnnonces = false;
            NotifyStateChanged();
        }
    }

    public async Task SelectAnnonceFilter(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out int idAnnonce))
        {
            idAnnonce = 0;
        }
        
        if (SelectedAnnonceFilter == idAnnonce)
            return; // Pas de changement
        
        SelectedAnnonceFilter = idAnnonce;
        SelectedConversation = null;
        Messages.Clear();
        
        Console.WriteLine($"🔍 Filtrage par annonce: {(idAnnonce == 0 ? "Toutes" : idAnnonce.ToString())}");
        
        await LoadConversations(idAnnonce);
    }

    public async Task SelectConversation(ConversationListDTO conv)
    {
        SelectedConversation = conv;
        CommandeEnCours = null;
        await LoadMessages(conv.IdConversation);
        await LoadOffre(conv.IdConversation);
        await ABloquer(true);
        NotifyStateChanged();
        
        if (OnScrollRequested != null)
        {
            await Task.Delay(100);
            await OnScrollRequested.Invoke();
        }
    }

    private async Task LoadOffre(int conversationId)
    {
        try
        {
            CommandeEnCours = await _commandeService.GetCommandeByIdConv(conversationId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement commande: {ex.Message}");
        }
    }

    private async void HandleOffreStatusChanged(int idOffre, bool? estAccepte)
    {
        if (SelectedConversation == null) return;

        var message = Messages.FirstOrDefault(m =>
            m.Offres != null && m.Offres.Any(o => o.IdOffre == idOffre));

        if (message != null && message.Offres != null)
        {
            var offre = message.Offres.FirstOrDefault(o => o.IdOffre == idOffre);
            if (offre != null)
            {
                offre.EstAccepte = estAccepte;
                try
                {
                    CommandeEnCours = await _commandeService.GetCommandeByIdConv(SelectedConversation.IdConversation);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur chargement commande: {ex.Message}");
                }
                Console.WriteLine($"✅ Offre {idOffre} mise à jour en temps réel");
                await _conversationState.ReloadConversationsAsync();
                await LoadConversations(SelectedAnnonceFilter);
            }
        }
    }

    private async Task LoadMessages(int conversationId, bool doScroll = false)
    {
        IsLoadingMessages = true;
        NotifyStateChanged();

        try
        {
            // Chercher dans ConversationsFiltered au lieu de Conversations
            var conv = ConversationsFiltered.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null && conv.NombreNonLu > 0)
            {
                Console.WriteLine($"📭 Marquage de {conv.NombreNonLu} messages comme lus");
                conv.NombreNonLu = 0;
                
                // Mettre à jour aussi dans Conversations global si présent
                var globalConv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
                if (globalConv != null)
                {
                    globalConv.NombreNonLu = 0;
                }
                
                _conversationState.NotifyMessagesRead();
            }

            Messages = (await _messageService.GetMessagesByConversationAndMarkAsRead(conversationId, CurrentUserId))
                .ToList();
            Console.WriteLine($"✅ {Messages.Count} messages chargés pour conversation {conversationId}");
            
            if (doScroll && OnScrollRequested != null)
            {
                await Task.Delay(100);
                await OnScrollRequested.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement messages: {ex.Message}");
        }
        finally
        {
            IsLoadingMessages = false;
            NotifyStateChanged();
        }
    }

    public void OnFilesSelected(List<IBrowserFile> files)
    {
        SelectedFiles = files;
        NotifyStateChanged();
    }

    private async void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        if (SelectedConversation?.IdConversation == conversationId)
        {
            if (senderId == CurrentUserId)
            {
                var exists = Messages.Any(m => 
                    m.IdCompte == senderId && 
                    m.ContenuMessage == message && 
                    Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

                if (exists)
                {
                    Console.WriteLine($"⚠️ Message déjà présent localement (ignoré de SignalR)");
                    return;
                }
            }

            var newMsg = new MessageDTO
            {
                IdConversation = conversationId,
                IdCompte = senderId,
                ContenuMessage = message,
                DateEnvoiMessage = date,
                EstLu = false
            };

            var messageExists = Messages.Any(m => 
                m.IdCompte == senderId && 
                m.ContenuMessage == message && 
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!messageExists)
            {
                Messages.Add(newMsg);
                Console.WriteLine($"📨 Message reçu de {senderId}: {message.Substring(0, Math.Min(30, message.Length))}...");
                NotifyStateChanged();
            
                if (OnScrollRequested != null)
                {
                    await OnScrollRequested.Invoke();
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Message déjà présent (doublon SignalR évité)");
            }
        }
        else
        {
            await _conversationState.ReloadConversationsAsync();
            await LoadConversations(SelectedAnnonceFilter);
            NotifyStateChanged();
        }
    }

    private void HandleMessagesRead(int conversationId, int userIdReader)
    {
        if (SelectedConversation?.IdConversation != conversationId)
            return;

        bool stateChanged = false;

        if (userIdReader != CurrentUserId)
        {
            var myUnreadMessages = Messages
                .Where(m => m.IdCompte == CurrentUserId && !m.EstLu);

            foreach (var msg in myUnreadMessages)
            {
                msg.EstLu = true;
                stateChanged = true;
            }
        }
        else
        {
            var otherUnreadMessages = Messages
                .Where(m => m.IdCompte != CurrentUserId && !m.EstLu);

            foreach (var msg in otherUnreadMessages)
            {
                msg.EstLu = true;
                stateChanged = true;
            }
        }

        if (stateChanged)
        {
            NotifyStateChanged();
        }
    }

    private void HandleUserTyping(int conversationId, int userId, string userName)
    {
        if (SelectedConversation?.IdConversation != conversationId || userId == CurrentUserId)
            return;

        IsTyping = true;
        NotifyStateChanged();

        Task.Delay(3000).ContinueWith(_ =>
        {
            IsTyping = false;
            NotifyStateChanged();
        });
    }

    private void HandleGlobalStateChanged()
    {
        NotifyStateChanged();
    }

    public void HandleTyping(KeyboardEventArgs e)
    {
        if (SelectedConversation == null)
            return;

        _typingTimer?.Dispose();
        _typingTimer = new System.Threading.Timer(_ =>
        {
            _typingNotified = false;
        }, null, 2000, Timeout.Infinite);

        if (_typingNotified)
            return;

        _typingNotified = true;

        _ = _signalR.NotifyTyping(SelectedConversation.IdConversation, CurrentUserId, "User");
    }

    public async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewMessage))
        {
            await SendMessageWithOffre();
        }
    }

    private void NotifyStateChanged()
    {
        _refreshUI?.Invoke();
    }
    
    public void RemoveFile(IBrowserFile file)
    {
        SelectedFiles.Remove(file);
        NotifyStateChanged();
    }
    
    public async Task ABloquer(bool premierBloque)
    {
        EstBloquer = await _bloqueService.ABloque(CurrentUserId, SelectedConversation.IdParticipant, premierBloque);
        Console.WriteLine($"EstBloquer: {EstBloquer}, premierBloque: {premierBloque}");
    
        if (EstBloquer)
        {
            if (premierBloque)
            {
                BlocageType = "je suis bloquer";
            }
            else
            {
                BlocageType = "est bloquer";
            }
        }
        else
        {
            if (premierBloque)
            {
                EstBloquer = await _bloqueService.ABloque(CurrentUserId, SelectedConversation.IdParticipant, false);
            
                if (EstBloquer)
                {
                    BlocageType = "est bloquer";
                }
                else
                {
                    BlocageType = "";
                }
            }
            else
            {
                BlocageType = "";
            }
        }

        _refreshUI?.Invoke();
    }
    
    public void OnInputChanged(ChangeEventArgs e)
    {
        NewMessage = e.Value?.ToString() ?? "";
    }

    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _signalR.OnUserTyping -= HandleUserTyping;
        _signalR.OnMessagesRead -= HandleMessagesRead;
        _conversationState.OnStateChanged -= HandleGlobalStateChanged;
        _signalR.OnOffreStatusChanged -= HandleOffreStatusChanged;
        _signalR.OnMessageWithOffreReceived -= HandleMessageWithOffreReceived;
        _typingTimer?.Dispose();
    }

    public async Task ToggleOffreMode()
    {
        ShowOffreMode = !ShowOffreMode;

        if (ShowOffreMode && SelectedConversation != null)
        {
            try
            {
                var annonce = await _annonceService.GetAnnonceDetailById(SelectedConversation.IdAnnonce);
                if (annonce != null)
                {
                    AnnonceIdForOffre = annonce.IdAnnonce;
                    AnnoncePrixMax = annonce.Prix;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur chargement annonce: {ex.Message}");
            }
        }
        else
        {
            OffreAmount = 0;
            OffreError = "";
            OffreInfoMessage = "";
        }

        NotifyStateChanged();
    }

    public void UpdateOffreAmount(string value)
    {
        if (decimal.TryParse(value, out var amount))
        {
            OffreAmount = amount;
            OffreError = "";

            if (AnnoncePrixMax.HasValue && AnnoncePrixMax.Value > 0)
            {
                var diff = AnnoncePrixMax.Value - OffreAmount;
                var percentage = (Math.Abs(diff) / AnnoncePrixMax.Value) * 100;

                if (diff > 0)
                {
                    OffreInfoMessage = $"Réduction de {diff:N0} € (-{percentage:F1}%)";
                    OffreInfoClass = "offre-reduction";
                }
                else if (diff < 0)
                {
                    OffreInfoMessage = $"Augmentation de {Math.Abs(diff):N0} € (+{percentage:F1}%)";
                    OffreInfoClass = "offre-increase";
                }
                else
                {
                    OffreInfoMessage = "Prix identique à l'annonce";
                    OffreInfoClass = "offre-neutral";
                }
            }
        }
        else
        {
            OffreError = "Montant invalide";
            OffreInfoMessage = "";
        }
        NotifyStateChanged();
    }

    public async Task SendMessageWithOffre()
    {
        if (SelectedConversation == null)
            return;

        bool hasContent = !string.IsNullOrWhiteSpace(NewMessage) ||
                          (ShowOffreMode && OffreAmount > 0) ||
                          SelectedFiles.Any();

        if (!hasContent)
            return;

        if (ShowOffreMode && OffreAmount <= 0)
        {
            OffreError = "Le montant doit être supérieur à 0";
            NotifyStateChanged();
            return;
        }

        var messageText = string.IsNullOrWhiteSpace(NewMessage)
            ? (ShowOffreMode ? $"💰 Offre de {OffreAmount:N0} €" : "[Fichier(s) joint(s)]")
            : NewMessage.Trim();

        var filesToUpload = new List<IBrowserFile>(SelectedFiles);
        var offreAmountToSend = ShowOffreMode ? OffreAmount : 0;
        var annonceIdToSend = AnnonceIdForOffre;

        _newMessage = "";
        OffreAmount = 0;
        OffreError = "";
        OffreInfoMessage = "";
        ShowOffreMode = false;
        SelectedFiles.Clear();
        NotifyStateChanged();

        try
        {
            IsUploadingFiles = true;

            var messageDto = new MessageCreateDTO
            {
                IdConversation = SelectedConversation.IdConversation,
                IdCompte = CurrentUserId,
                ContenuMessage = messageText,
            };

            var result = await _messageService.CreateMessageAsync(messageDto, offreAmountToSend > 0 && annonceIdToSend.HasValue);

            if (!result.Success)
            {
                Console.WriteLine($"❌ Erreur API : {result.ErrorMessage}");
                _newMessage = messageText;
                _notificationService.ShowError("Erreur lors de l'envoie du message", result.ErrorMessage);
                NotifyStateChanged();
                return;
            }

            var createdMessage = result.Data;

            if (createdMessage == null)
            {
                Console.WriteLine("❌ Erreur : message non créé");
                _newMessage = messageText;
                NotifyStateChanged();
                return;
            }

            if (offreAmountToSend > 0 && annonceIdToSend.HasValue)
            {
                try
                {
                    var offreDto = new OffreCreateDTO
                    {
                        IdAnnonce = annonceIdToSend.Value,
                        IdMessage = createdMessage.IdMessage,
                        Valeur = offreAmountToSend
                    };

                    var offreCreee = await _offreService.CreateAsync(offreDto);

                    if (offreCreee != null)
                    {
                        Console.WriteLine($"✅ Offre {offreCreee.IdOffre} de {offreAmountToSend:N0} € créée");

                        await LoadMessages(SelectedConversation.IdConversation);
                        
                        if (OnScrollRequested != null)
                        {
                            await OnScrollRequested.Invoke();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur création offre: {ex.Message}");
                }
            }

            if (filesToUpload.Any())
            {
                try
                {
                    Console.WriteLine($"📤 Upload de {filesToUpload.Count} fichier(s)...");

                    var uploadedFiles = await _pieceJointeService.UploadFilesAsync(
                        createdMessage.IdMessage,
                        filesToUpload);

                    Console.WriteLine($"✅ {uploadedFiles.Count} fichier(s) uploadé(s)");

                    await LoadMessages(SelectedConversation.IdConversation, doScroll: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Erreur upload: {ex.Message}");
                    _notificationService.ShowError("Erreur upload", "Impossible d'envoyer les fichiers");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur envoi message: {ex.Message}");
            _newMessage = messageText;
        }
        finally
        {
            IsUploadingFiles = false;
            await _conversationState.ReloadConversationsAsync();
            await LoadConversations(SelectedAnnonceFilter);
            NotifyStateChanged();
        }
    }

    public async Task AccepterOffre(int idOffre)
    {
        try
        {
            var success = await _offreService.AccepterOffreAsync(idOffre);
            if (success)
            {
                Console.WriteLine($"✅ Offre {idOffre} acceptée");
                if (SelectedConversation != null)
                {
                    await LoadMessages(SelectedConversation.IdConversation);
                    await _conversationState.ReloadConversationsAsync();
                    await LoadConversations(SelectedAnnonceFilter);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur acceptation offre: {ex.Message}");
        }
    }

    public async Task RefuserOffre(int idOffre)
    {
        try
        {
            var success = await _offreService.RefuserOffreAsync(idOffre);
            if (success)
            {
                Console.WriteLine($"✅ Offre {idOffre} refusée");
                if (SelectedConversation != null)
                {
                    await LoadMessages(SelectedConversation.IdConversation);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur refus offre: {ex.Message}");
        }
    }

    private async void HandleMessageWithOffreReceived(
        int conversationId,
        int senderId,
        string message,
        DateTime date,
        int idMessage,
        int idOffre,
        decimal offreValeur,
        int idAnnonce)
    {
        if (SelectedConversation?.IdConversation == conversationId)
        {
            var exists = Messages.Any(m =>
                m.IdCompte == senderId &&
                m.ContenuMessage == message &&
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!exists)
            {
                var newMsg = new MessageDTO
                {
                    IdMessage = idMessage,
                    IdConversation = conversationId,
                    IdCompte = senderId,
                    ContenuMessage = message,
                    DateEnvoiMessage = date,
                    EstLu = senderId == CurrentUserId,
                    Offres = new List<OffreDTO>
                    {
                        new OffreDTO
                        {
                            IdOffre = idOffre,
                            IdMessage = idMessage,
                            Valeur = offreValeur,
                            IdAnnonce = idAnnonce,
                            DateOffre = date,
                            EstAccepte = null
                        }
                    }
                };
                
                Messages.Add(newMsg);
                NotifyStateChanged();

                if (OnScrollRequested != null)
                {
                    await OnScrollRequested.Invoke();
                }
            }
        }
        
        await _conversationState.ReloadConversationsAsync();
        await LoadConversations(SelectedAnnonceFilter);
    }
}