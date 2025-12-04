using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.Http.Json;

namespace BlazorAutoPulse.ViewModel;

public class ConversationsViewModel
{
    private readonly ISignalRService _signalR;
    private readonly IConversationService _conversationService;
    private readonly IService<MessageDTO> _messageService;
    private readonly ICompteService _compteService;
    private readonly NavigationManager _navigation;
    private readonly IImageService _imageService;
    private readonly HttpClient _httpClient;

    public List<ConversationListDTO> Conversations { get; private set; } = new();
    public List<MessageDTO> Messages { get; private set; } = new();
    public ConversationListDTO? SelectedConversation { get; private set; }
    
    public string NewMessage { get; set; } = "";
    public int CurrentUserId { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public bool IsTyping { get; private set; } = false;
    
    private string mimeType = "data:image/jpeg;base64,";
    public Dictionary<int, string> ImageSources { get; private set; } = new();

    public ElementReference MessagesContainer;
    private System.Threading.Timer? _typingTimer;

    public event Action? _refreshUI;

    public ConversationsViewModel(
        ISignalRService signalR,
        IConversationService convService,
        IService<MessageDTO> msgService,
        ICompteService compteService,
        IImageService imageService,
        HttpClient httpClient,
        NavigationManager nav)
    {
        _signalR = signalR;
        _conversationService = convService;
        _messageService = msgService;
        _compteService = compteService;
        _imageService = imageService;
        _httpClient = httpClient;
        _navigation = nav;

        _signalR.OnMessageReceived += HandleMessageReceived;
        _signalR.OnUserTyping += HandleUserTyping;
        _signalR.OnMessagesRead += HandleMessagesRead;
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _signalR.StartAsync();
            
            var compteDetail = await _compteService.GetMe();
            CurrentUserId = compteDetail.IdCompte;
            Console.WriteLine("1");
            await LoadConversations();
            Console.WriteLine("2");
            foreach (var conv in Conversations)
            {
                Console.WriteLine("3");
                await _signalR.JoinConversation(conv.IdConversation);
                Console.WriteLine("4");
                await GetImageProfil(conv.IdParticipant);
                Console.WriteLine("5");
            }
            
            _refreshUI?.Invoke();
        }
        catch (Exception ex)
        {
            _navigation.NavigateTo("/connexion");
        }
        finally
        {
            IsLoading = false;
            _refreshUI?.Invoke();
        }
    }

    private async Task LoadConversations()
    {
        Conversations = (await _conversationService.GetConversationsByCompteID(CurrentUserId)).ToList();
    }
    
    public async Task SelectConversation(ConversationListDTO conv)
    {
        SelectedConversation = conv;

        await LoadMessages(conv.IdConversation);

        _refreshUI?.Invoke();
    }

    private async Task LoadMessages(int conversationId)
    {
        try
        {
            var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null)
            {
                conv.NombreNonLu = 0;
            }

            Messages = (await _messageService.GetAllAsync())
                .Where(m => m.IdConversation == conv.IdConversation)
                .OrderBy(m => m.DateEnvoiMessage)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors du chargement des messages: {ex.Message}");
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
            NewMessage = messageContent;
        }

        _refreshUI?.Invoke();
    }

    private void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        // Ajouter le message à la liste si c'est la conversation active
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
            }
        }
        else
        {
            // Incrémenter le compteur de messages non lus pour cette conversation
            var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
            if (conv != null && senderId != CurrentUserId)
            {
                conv.NombreNonLu++;
            }
        }

        _refreshUI?.Invoke();
    }

    private void HandleMessagesRead(int conversationId, int userId)
    {
        // Si c'est nous qui avons lu, on met à jour les messages
        if (userId == CurrentUserId && SelectedConversation?.IdConversation == conversationId)
        {
            foreach (var msg in Messages.Where(m => m.IdCompte != CurrentUserId))
            {
                msg.EstLu = true;
            }
            _refreshUI?.Invoke();
        }
    }

    private void HandleUserTyping(int conversationId, int userId, string userName)
    {
        if (SelectedConversation?.IdConversation != conversationId || userId == CurrentUserId)
            return;

        IsTyping = true;
        _refreshUI?.Invoke();

        Task.Delay(3000).ContinueWith(_ =>
        {
            IsTyping = false;
            _refreshUI?.Invoke();
        });
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
    
    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _signalR.OnUserTyping -= HandleUserTyping;
        _signalR.OnMessagesRead -= HandleMessagesRead;
        _typingTimer?.Dispose();
    }
    
    public async Task GetImageProfil(int idCompte)
    {
        try
        {
            Image? img = await _imageService.GetImageProfil(idCompte);

            string imageSource = "";
            if (img != null && img.Fichier != null && img.Fichier.Length > 0)
            {
                var base64 = Convert.ToBase64String(img.Fichier);
                imageSource = $"{mimeType}{base64}";
            }
            else
            {
                imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
            }

            ImageSources[idCompte] = imageSource;
            _refreshUI?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lors de l'affichage de l'image : {ex.Message}");
            ImageSources[idCompte] = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
            _refreshUI?.Invoke();
        }
    }

    public int GetTotalUnreadCount()
    {
        return Conversations.Sum(c => c.NombreNonLu);
    }
}