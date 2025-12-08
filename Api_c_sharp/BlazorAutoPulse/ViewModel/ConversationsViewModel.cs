using AutoPulse.Shared.DTO;
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
    private readonly IJSRuntime _jsRuntime;

    public List<MessageDTO> Messages { get; private set; } = new();
    public ConversationListDTO? SelectedConversation { get; private set; }
    public string NewMessage { get; set; } = "";
    public bool IsTyping { get; private set; } = false;
    public ElementReference MessagesContainer;

    public List<IBrowserFile> SelectedFiles { get; set; } = new();
    public bool IsUploadingFiles { get; set; } = false;
    
    // ✅ NOUVEAU : Loading pour les messages
    public bool IsLoadingMessages { get; private set; } = false;

    private System.Threading.Timer? _typingTimer;
    private bool _typingNotified = false;
    public event Action? _refreshUI;

    public List<ConversationListDTO> Conversations => _conversationState.Conversations;
    public int CurrentUserId => _conversationState.CurrentUserId;
    public bool IsLoading => _conversationState.IsLoading;
    public Dictionary<int, string> ImageSources => _conversationState.ImageSources;

    public ConversationViewModel(
        ConversationStateService conversationState,
        ISignalRService signalR,
        IMessageService msgService,
        IPieceJointeService pieceJointeService,
        IJSRuntime jsRuntime)
    {
        _conversationState = conversationState;
        _signalR = signalR;
        _messageService = msgService;
        _pieceJointeService = pieceJointeService;
        _jsRuntime = jsRuntime;

        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
        _conversationState.OnStateChanged += HandleGlobalStateChanged;
    }

    public async Task InitializeAsync()
    {
        await _conversationState.InitializeAsync();
    }

    public async Task SelectConversation(ConversationListDTO conv)
    {
        SelectedConversation = conv;
        await LoadMessages(conv.IdConversation);
        NotifyStateChanged();
        await ScrollToBottom();
    }

    private async Task LoadMessages(int conversationId)
    {
        // ✅ Activer le loading
        IsLoadingMessages = true;
        NotifyStateChanged();

        try
        {
            var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null && conv.NombreNonLu > 0)
            {
                Console.WriteLine($"📭 Marquage de {conv.NombreNonLu} messages comme lus");
                conv.NombreNonLu = 0;
                _conversationState.NotifyMessagesRead();
            }

            Messages = (await _messageService.GetMessagesByConversationAndMarkAsRead(conversationId, CurrentUserId)).ToList();
            Console.WriteLine($"✅ {Messages.Count} messages chargés pour conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur chargement messages: {ex.Message}");
        }
        finally
        {
            // ✅ Désactiver le loading
            IsLoadingMessages = false;
            NotifyStateChanged();
        }
    }

    // ✅ Méthode CORRIGÉE pour gérer les fichiers avec rafraîchissement
    public async Task SendMessage()
    {
        if (SelectedConversation == null)
            return;

        // Vérifier qu'il y a du texte OU des fichiers
        if (string.IsNullOrWhiteSpace(NewMessage) && !SelectedFiles.Any())
            return;

        var messageContent = NewMessage.Trim();
        NewMessage = "";
        var filesToUpload = new List<IBrowserFile>(SelectedFiles);
        SelectedFiles.Clear();

        try
        {
            IsUploadingFiles = true;
            NotifyStateChanged();

            // 1. Créer le message
            var messageDto = new MessageDTO
            {
                IdConversation = SelectedConversation.IdConversation,
                IdCompte = CurrentUserId,
                ContenuMessage = string.IsNullOrWhiteSpace(messageContent) ? "[Fichier(s) joint(s)]" : messageContent,
            };

            var createdMessage = await _messageService.CreateAsync(messageDto);

            if (createdMessage == null)
            {
                Console.WriteLine("❌ Erreur : message non créé");
                NewMessage = messageContent;
                SelectedFiles = filesToUpload;
                return;
            }

            Console.WriteLine($"✅ Message créé avec ID: {createdMessage.IdMessage}");

            // 2. Upload des fichiers si présents
            if (filesToUpload.Any())
            {
                Console.WriteLine($"📤 Upload de {filesToUpload.Count} fichier(s)...");

                var uploadedFiles = await _pieceJointeService.UploadFilesAsync(
                    createdMessage.IdMessage, 
                    filesToUpload);

                Console.WriteLine($"✅ {uploadedFiles.Count} fichier(s) uploadé(s)");

                // ✅ CORRECTION : Attacher les fichiers au message créé
                createdMessage.PiecesJointes = uploadedFiles;
            }

            // ✅ CORRECTION : Ajouter le message complet avec ses pièces jointes à la liste
            var messageExists = Messages.Any(m => m.IdMessage == createdMessage.IdMessage);
            if (!messageExists)
            {
                Messages.Add(createdMessage);
                Console.WriteLine($"✅ Message ajouté à la liste locale avec {createdMessage.PiecesJointes?.Count() ?? 0} pièce(s) jointe(s)");
            }
            else
            {
                // Si le message existe déjà (via SignalR), mettre à jour ses pièces jointes
                var existingMessage = Messages.First(m => m.IdMessage == createdMessage.IdMessage);
                existingMessage.PiecesJointes = createdMessage.PiecesJointes;
                Console.WriteLine($"✅ Message existant mis à jour avec {createdMessage.PiecesJointes?.Count() ?? 0} pièce(s) jointe(s)");
            }

            // ✅ Forcer le rafraîchissement de l'UI
            NotifyStateChanged();

            await Task.Delay(100);
            await ScrollToBottom();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur envoi message: {ex.Message}");
            NewMessage = messageContent;
            SelectedFiles = filesToUpload;
        }
        finally
        {
            IsUploadingFiles = false;
            NotifyStateChanged();
        }
    }

    // ✅ Gérer la sélection de fichiers
    public void OnFilesSelected(List<IBrowserFile> files)
    {
        SelectedFiles = files;
        NotifyStateChanged();
    }

    private async void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        if (SelectedConversation?.IdConversation == conversationId)
        {
            var newMsg = new MessageDTO
            {
                IdConversation = conversationId,
                IdCompte = senderId,
                ContenuMessage = message,
                DateEnvoiMessage = date,
                EstLu = senderId == CurrentUserId
            };

            var exists = Messages.Any(m => 
                m.IdCompte == senderId && 
                m.ContenuMessage == message && 
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!exists)
            {
                Messages.Add(newMsg);
                NotifyStateChanged();
                await Task.Delay(100);
                await ScrollToBottom();
            }
        }
    }

    private void HandleMessagesRead(int conversationId, int userId)
    {
        if (userId == CurrentUserId && SelectedConversation?.IdConversation == conversationId)
        {
            foreach (var msg in Messages.Where(m => m.IdCompte != CurrentUserId))
            {
                msg.EstLu = true;
            }
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

    public async Task HandleTyping(KeyboardEventArgs e)
    {
        if (SelectedConversation == null)
            return;

        _typingTimer?.Dispose();
        _typingTimer = new System.Threading.Timer(async _ =>
        {
            _typingNotified = false;
        }, null, 2000, Timeout.Infinite);

        if (_typingNotified)
            return;

        _typingNotified = true;

        await _signalR.NotifyTyping(SelectedConversation.IdConversation, CurrentUserId, "User");
    }

    public async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewMessage))
            await SendMessage();
    }

    public async Task ScrollToBottom()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("scrollToBottom", MessagesContainer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur scroll: {ex.Message}");
        }
    }

    private void NotifyStateChanged() => _refreshUI?.Invoke();

    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _signalR.OnUserTyping -= HandleUserTyping;
        _signalR.OnMessagesRead -= HandleMessagesRead;
        _conversationState.OnStateChanged -= HandleGlobalStateChanged;
        _typingTimer?.Dispose();
    }
}