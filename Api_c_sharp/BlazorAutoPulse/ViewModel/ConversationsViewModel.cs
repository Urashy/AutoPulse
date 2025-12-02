using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorAutoPulse.ViewModel;

public class ConversationsViewModel
{
    private readonly ISignalRService _signalR;
    private readonly IService<ConversationDetailDTO> _conversationService;
    private readonly IService<MessageDTO> _messageService;
    private readonly ICompteService _compteService;
    private readonly NavigationManager _navigation;

    public List<ConversationDetailDTO> Conversations { get; private set; } = new();
    public List<MessageDTO> Messages { get; private set; } = new();
    public ConversationDetailDTO? SelectedConversation { get; private set; }
    
    public string NewMessage { get; set; } = "";
    public int CurrentUserId { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public bool IsTyping { get; private set; } = false;

    public ElementReference MessagesContainer;
    private System.Threading.Timer? _typingTimer;

    public event Action? OnRefresh;

    public ConversationsViewModel(
        ISignalRService signalR,
        IService<ConversationDetailDTO> convService,
        IService<MessageDTO> msgService,
        ICompteService compteService,
        NavigationManager nav)
    {
        _signalR = signalR;
        _conversationService = convService;
        _messageService = msgService;
        _compteService = compteService;
        _navigation = nav;

        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
    }

    // -------------------------------------------------
    // INITIALISATION
    // -------------------------------------------------
    public async Task InitializeAsync()
    {
        try
        {
            var me = await _compteService.GetMe();
            CurrentUserId = me.IdCompte;

            Conversations = (await _conversationService.GetAllAsync()).ToList();

            foreach (var conv in Conversations)
                await _signalR.JoinConversation(conv.IdConversation);
        }
        catch
        {
            _navigation.NavigateTo("/connexion");
        }
        finally
        {
            IsLoading = false;
            OnRefresh?.Invoke();
        }
    }

    // -------------------------------------------------
    // SELECTION D'UNE CONVERSATION
    // -------------------------------------------------
    public async Task SelectConversation(ConversationDetailDTO conv)
    {
        SelectedConversation = conv;

        Messages = (await _messageService.GetAllAsync())
            .Where(m => m.IdConversation == conv.IdConversation)
            .OrderBy(m => m.DateEnvoiMessage)
            .ToList();

        await _signalR.MarkAsRead(conv.IdConversation, CurrentUserId);

        OnRefresh?.Invoke();
    }

    // -------------------------------------------------
    // ENVOI MESSAGE
    // -------------------------------------------------
    public async Task SendMessage()
    {
        if (SelectedConversation == null || string.IsNullOrWhiteSpace(NewMessage))
            return;

        await _signalR.SendMessage(
            SelectedConversation.IdConversation,
            CurrentUserId,
            NewMessage.Trim());

        await _messageService.CreateAsync(new MessageDTO
        {
            IdConversation = SelectedConversation.IdConversation,
            IdCompte = CurrentUserId,
            ContenuMessage = NewMessage.Trim(),
            DateEnvoiMessage = DateTime.UtcNow
        });

        NewMessage = "";
        OnRefresh?.Invoke();
    }

    // -------------------------------------------------
    // SIGNALR EVENTS
    // -------------------------------------------------
    private void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        if (SelectedConversation?.IdConversation != conversationId)
            return;

        Messages.Add(new MessageDTO
        {
            IdConversation = conversationId,
            IdCompte = senderId,
            ContenuMessage = message,
            DateEnvoiMessage = date
        });

        OnRefresh?.Invoke();
    }

    private void HandleUserTyping(int conversationId, int userId, string userName)
    {
        if (SelectedConversation?.IdConversation != conversationId || userId == CurrentUserId)
            return;

        IsTyping = true;
        OnRefresh?.Invoke();

        Task.Delay(3000).ContinueWith(_ =>
        {
            IsTyping = false;
            OnRefresh?.Invoke();
        });
    }

    // -------------------------------------------------
    // TYPING
    // -------------------------------------------------
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

    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _signalR.OnUserTyping -= HandleUserTyping;
        _typingTimer?.Dispose();
    }
}