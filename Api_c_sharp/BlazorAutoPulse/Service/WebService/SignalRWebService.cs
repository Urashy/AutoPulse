using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorAutoPulse.Service.WebService;

public class SignalRWebService : ISignalRService, IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private bool _isStarted = false;

    // Événements publics
    public event Action<int, int, string, DateTime>? OnMessageReceived;
    public event Action<int, int, string>? OnUserTyping;
    public event Action<int, int>? OnMessagesRead;

    public SignalRWebService()
    {
        // Construire l'URL du Hub SignalR
        var hubUrl = "http://localhost:5086/messagehub";

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Pour Blazor WebAssembly: permettre l'envoi des cookies
                options.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Text;
                
                // Important pour envoyer les cookies d'authentification automatiquement
                options.HttpMessageHandlerFactory = (handler) =>
                {
                    return handler;
                };
            })
            .WithAutomaticReconnect(new[] 
            { 
                TimeSpan.Zero, 
                TimeSpan.FromSeconds(2), 
                TimeSpan.FromSeconds(5), 
                TimeSpan.FromSeconds(10) 
            })
            .Build();

        // S'abonner aux événements du Hub
        RegisterHubEvents();
    }

    private void RegisterHubEvents()
    {
        // Écouter "ReceiveMessage" depuis le serveur
        _hubConnection.On<int, int, string, DateTime>("ReceiveMessage", 
            (conversationId, senderId, message, dateTime) =>
            {
                Console.WriteLine($"🔔 SignalR Event: ReceiveMessage - Conv={conversationId}, Sender={senderId}, Msg={message}");
                OnMessageReceived?.Invoke(conversationId, senderId, message, dateTime);
            });

        // Écouter "UserIsTyping"
        _hubConnection.On<int, int, string>("UserIsTyping", 
            (conversationId, userId, userName) =>
            {
                Console.WriteLine($"⌨️ SignalR Event: UserIsTyping - Conv={conversationId}, User={userId}");
                OnUserTyping?.Invoke(conversationId, userId, userName);
            });

        // Écouter "MessagesRead"
        _hubConnection.On<int, int>("MessagesRead", 
            (conversationId, userId) =>
            {
                Console.WriteLine($"👁️ SignalR Event: MessagesRead - Conv={conversationId}, User={userId}");
                OnMessagesRead?.Invoke(conversationId, userId);
            });

        // Log des changements de connexion
        _hubConnection.Reconnecting += error =>
        {
            Console.WriteLine($"🔄 SignalR: Reconnecting... {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine($"✅ SignalR: Reconnected! ConnectionId={connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            Console.WriteLine($"❌ SignalR: Connection closed. {error?.Message}");
            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        if (_isStarted)
        {
            Console.WriteLine("⚠️ SignalR: Already started");
            return;
        }

        try
        {
            Console.WriteLine("🔌 SignalR: Tentative de connexion...");
            await _hubConnection.StartAsync();
            _isStarted = true;
            Console.WriteLine($"✅ SignalR: Connected! State={_hubConnection.State}, ConnectionId={_hubConnection.ConnectionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SignalR: Failed to start - {ex.Message}");
            Console.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection.State == HubConnectionState.Connected)
        {
            await _hubConnection.StopAsync();
            _isStarted = false;
            Console.WriteLine("🛑 SignalR: Disconnected");
        }
    }

    public async Task JoinConversation(int conversationId)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
        {
            Console.WriteLine($"⚠️ SignalR: Not connected (State={_hubConnection.State}), cannot join conversation {conversationId}");
            return;
        }

        try
        {
            await _hubConnection.InvokeAsync("JoinConversation", conversationId);
            Console.WriteLine($"✅ Joined conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error joining conversation {conversationId}: {ex.Message}");
        }
    }

    public async Task LeaveConversation(int conversationId)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
            return;

        try
        {
            await _hubConnection.InvokeAsync("LeaveConversation", conversationId);
            Console.WriteLine($"👋 Left conversation {conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error leaving conversation: {ex.Message}");
        }
    }

    public async Task SendMessage(int conversationId, int senderId, string message)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
        {
            Console.WriteLine("⚠️ SignalR: Not connected, cannot send message");
            return;
        }

        try
        {
            await _hubConnection.InvokeAsync("SendMessage", conversationId, senderId, message);
            Console.WriteLine($"📤 Message sent via SignalR: Conv={conversationId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error sending message: {ex.Message}");
        }
    }

    public async Task NotifyTyping(int conversationId, int userId, string userName)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
            return;

        try
        {
            await _hubConnection.InvokeAsync("UserTyping", conversationId, userId, userName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error notifying typing: {ex.Message}");
        }
    }

    public async Task MarkAsRead(int conversationId, int userId)
    {
        if (_hubConnection.State != HubConnectionState.Connected)
            return;

        try
        {
            await _hubConnection.InvokeAsync("MarkAsRead", conversationId, userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error marking as read: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        await _hubConnection.DisposeAsync();
    }
}