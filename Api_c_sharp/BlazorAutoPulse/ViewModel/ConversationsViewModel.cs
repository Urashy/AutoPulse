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
    
    private string _newMessage = "";
    public string NewMessage 
    { 
        get => _newMessage;
        set => _newMessage = value; // ✅ AUCUN NotifyStateChanged
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
    }

    private async Task LoadMessages(int conversationId)
    {
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
            IsLoadingMessages = false;
            NotifyStateChanged();
        }
    }

    public async Task SendMessage()
    {
        if (SelectedConversation == null)
            return;

        if (string.IsNullOrWhiteSpace(NewMessage) && !SelectedFiles.Any())
            return;

        var messageContent = NewMessage.Trim();
        var filesToUpload = new List<IBrowserFile>(SelectedFiles);
        
        // ✅ Nettoyer immédiatement
        _newMessage = "";
        SelectedFiles.Clear();
        
        // ✅ UN SEUL rafraîchissement
        NotifyStateChanged();

        try
        {
            IsUploadingFiles = true;

            // Créer le message texte
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
                _newMessage = messageContent;
                SelectedFiles = filesToUpload;
                NotifyStateChanged();
                return;
            }

            // ✅ Upload des fichiers en arrière-plan (ne pas bloquer l'UI)
            if (filesToUpload.Any())
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        Console.WriteLine($"📤 Upload de {filesToUpload.Count} fichier(s)...");
                        var uploadedFiles = await _pieceJointeService.UploadFilesAsync(
                            createdMessage.IdMessage, 
                            filesToUpload);

                        // Mettre à jour le message avec les pièces jointes
                        createdMessage.PiecesJointes = uploadedFiles;
                        Console.WriteLine($"✅ {uploadedFiles.Count} fichier(s) uploadé(s)");
                        
                        // Rafraîchir seulement après l'upload
                        NotifyStateChanged();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur upload: {ex.Message}");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur envoi message: {ex.Message}");
            _newMessage = messageContent;
            SelectedFiles = filesToUpload;
        }
        finally
        {
            IsUploadingFiles = false;
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
            // ✅ CORRECTION : Si c'est notre propre message, vérifier par ID exact
            if (senderId == CurrentUserId)
            {
                // Notre propre message : vérifier s'il existe déjà par contenu ET date
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
                EstLu = senderId == CurrentUserId
            };

            // Pour les messages des autres, vérifier aussi
            var messageExists = Messages.Any(m => 
                m.IdCompte == senderId && 
                m.ContenuMessage == message && 
                Math.Abs((m.DateEnvoiMessage - date).TotalSeconds) < 2);

            if (!messageExists)
            {
                Messages.Add(newMsg);
                Console.WriteLine($"📨 Message reçu de {senderId}: {message.Substring(0, Math.Min(30, message.Length))}...");
                NotifyStateChanged();
            }
            else
            {
                Console.WriteLine($"⚠️ Message déjà présent (doublon SignalR évité)");
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

    public void HandleTyping(KeyboardEventArgs e)
    {
        if (SelectedConversation == null)
            return;

        // ✅ Ne PAS notifier SignalR à chaque frappe
        _typingTimer?.Dispose();
        _typingTimer = new System.Threading.Timer(_ =>
        {
            _typingNotified = false;
        }, null, 2000, Timeout.Infinite);

        if (_typingNotified)
            return;

        _typingNotified = true;

        // ✅ Async sans await (fire-and-forget)
        _ = _signalR.NotifyTyping(SelectedConversation.IdConversation, CurrentUserId, "User");
    }

    public async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewMessage))
        {
            await SendMessage();
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
        _typingTimer?.Dispose();
    }
}