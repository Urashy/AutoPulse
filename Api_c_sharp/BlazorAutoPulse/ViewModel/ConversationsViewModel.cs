using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using BlazorAutoPulse.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorAutoPulse.ViewModel;

/// <summary>
/// ViewModel spécifique à la page Conversations
/// Gère l'affichage des messages et l'envoi, mais utilise le service singleton pour l'état global
/// </summary>
public class ConversationViewModel : IDisposable
{
    private readonly ConversationStateService _conversationState;
    private readonly ISignalRService _signalR;
    private readonly IMessageService _messageService;

    public List<MessageDTO> Messages { get; private set; } = new();
    public ConversationListDTO? SelectedConversation { get; private set; }
    public string NewMessage { get; set; } = "";
    public bool IsTyping { get; private set; } = false;
    public ElementReference MessagesContainer;

    private System.Threading.Timer? _typingTimer;
    public event Action? _refreshUI;

    // Déléguer les propriétés au service singleton
    public List<ConversationListDTO> Conversations => _conversationState.Conversations;
    public int CurrentUserId => _conversationState.CurrentUserId;
    public bool IsLoading => _conversationState.IsLoading;
    public Dictionary<int, string> ImageSources => _conversationState.ImageSources;

    public ConversationViewModel(
        ConversationStateService conversationState,
        ISignalRService signalR,
        IMessageService msgService)
    {
        _conversationState = conversationState;
        _signalR = signalR;
        _messageService = msgService;

        // S'abonner aux events
        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
        _conversationState.OnStateChanged += HandleGlobalStateChanged;
    }

    public async Task InitializeAsync()
    {
        // Appeler l'initialisation du service (ne fait rien si déjà fait)
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
        try
        {
            // Mettre à jour le compteur localement
            var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null && conv.NombreNonLu > 0)
            {
                Console.WriteLine($"📭 Marquage de {conv.NombreNonLu} messages comme lus");
        
                conv.NombreNonLu = 0;
        
                _conversationState.NotifyMessagesRead();
            }

            // Charger et marquer comme lus
            Messages = (await _messageService.GetMessagesByConversationAndMarkAsRead(conversationId, CurrentUserId)).ToList();
            Console.WriteLine($"Messages chargés pour conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement messages: {ex.Message}");
        }
    }

    public async Task SendMessage()
    {
        if (SelectedConversation == null || string.IsNullOrWhiteSpace(NewMessage))
            return;

        var messageContent = NewMessage.Trim();
        NewMessage = "";

        try
        {
            await _messageService.CreateAsync(new MessageDTO
            {
                IdConversation = SelectedConversation.IdConversation,
                IdCompte = CurrentUserId,
                ContenuMessage = messageContent,
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur envoi message: {ex.Message}");
            NewMessage = messageContent;
        }

        NotifyStateChanged();
    }

    private void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        // Si c'est la conversation active, ajouter le message
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
        _typingTimer = new System.Threading.Timer(_ => { }, null, 2000, Timeout.Infinite);

        await _signalR.NotifyTyping(SelectedConversation.IdConversation, CurrentUserId, "User");
    }

    public async Task HandleKeyPress(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(NewMessage))
            await SendMessage();
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