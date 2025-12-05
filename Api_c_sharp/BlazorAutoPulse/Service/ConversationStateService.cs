using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.Services;

public class ConversationStateService : IDisposable
{
    private readonly ISignalRService _signalR;
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;
    private readonly ICompteService _compteService;
    private readonly NavigationManager _navigation;
    private readonly IImageService _imageService;

    private bool _isInitialized = false;
    private bool _isInitializing = false;

    public List<ConversationListDTO> Conversations { get; private set; } = new();
    public int CurrentUserId { get; private set; }
    public bool IsLoading { get; private set; } = true;
    public Dictionary<int, string> ImageSources { get; private set; } = new();

    // ✅ Event qui notifie tous les abonnés
    public event Action? OnStateChanged;

    public ConversationStateService(
        ISignalRService signalR,
        IConversationService convService,
        IMessageService msgService,
        ICompteService compteService,
        IImageService imageService,
        NavigationManager nav)
    {
        _signalR = signalR;
        _conversationService = convService;
        _messageService = msgService;
        _compteService = compteService;
        _imageService = imageService;
        _navigation = nav;

        _signalR.OnMessageReceived += HandleMessageReceived;
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
        {
            Console.WriteLine("✓ ConversationStateService déjà initialisé");
            return;
        }

        if (_isInitializing)
        {
            Console.WriteLine("⏳ Initialisation déjà en cours, attente...");
            while (_isInitializing)
            {
                await Task.Delay(100);
            }
            return;
        }

        _isInitializing = true;
        Console.WriteLine("🔄 Initialisation de ConversationStateService...");

        try
        {
            if (!_signalR.IsConnected)
            {
                await _signalR.StartAsync();
                Console.WriteLine("✓ SignalR connecté");
            }

            var compteDetail = await _compteService.GetMe();
            CurrentUserId = compteDetail.IdCompte;
            Console.WriteLine($"✓ Utilisateur courant: {CurrentUserId}");

            await LoadConversations();
            Console.WriteLine($"✓ {Conversations.Count} conversations chargées");

            foreach (var conv in Conversations)
            {
                await _signalR.JoinConversation(conv.IdConversation);
                await GetImageProfil(conv.IdParticipant);
            }
            Console.WriteLine($"✓ Rejoint {Conversations.Count} conversations SignalR");

            _isInitialized = true;
            Console.WriteLine("✅ ConversationStateService initialisé avec succès");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erreur d'initialisation: {ex.Message}");
            _navigation.NavigateTo("/connexion");
        }
        finally
        {
            _isInitializing = false;
            IsLoading = false;
            NotifyStateChanged();
        }
    }

    public async Task LoadConversations()
    {
        Conversations = (await _conversationService.GetConversationsByCompteID(CurrentUserId)).ToList();
        NotifyStateChanged();
    }
    
    public void NotifyMessagesRead()
    {
        NotifyStateChanged();
    }

    private void HandleMessageReceived(int conversationId, int senderId, string message, DateTime date)
    {
        var conv = Conversations.FirstOrDefault(c => c.IdConversation == conversationId);
        
        if (conv != null && senderId != CurrentUserId)
        {
            conv.NombreNonLu++;
            
            Console.WriteLine($"📬 Nouveau message dans conversation {conversationId}");
            Console.WriteLine($"   Total non lus pour cette conv: {conv.NombreNonLu}");
            Console.WriteLine($"   Total global: {GetTotalUnreadCount()}");
            
            NotifyStateChanged();
        }
    }

    private async Task GetImageProfil(int idCompte)
    {
        if (ImageSources.ContainsKey(idCompte))
            return;

        try
        {
            Image? img = await _imageService.GetImageProfil(idCompte);
            string imageSource;

            if (img != null && img.Fichier != null && img.Fichier.Length > 0)
            {
                var base64 = Convert.ToBase64String(img.Fichier);
                imageSource = $"data:image/jpeg;base64,{base64}";
            }
            else
            {
                imageSource = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
            }

            ImageSources[idCompte] = imageSource;
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur chargement image {idCompte}: {ex.Message}");
            ImageSources[idCompte] = "https://st3.depositphotos.com/6672868/13701/v/450/depositphotos_137014128-stock-illustration-user-profile-icon.jpg";
        }
    }

    public int GetTotalUnreadCount()
    {
        return Conversations.Sum(c => c.NombreNonLu);
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    public void Dispose()
    {
        _signalR.OnMessageReceived -= HandleMessageReceived;
        _isInitialized = false;
        Console.WriteLine("🔌 ConversationStateService disposed");
    }
}