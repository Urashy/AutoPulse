using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorAutoPulse.Service.WebService;

public class SignalRWebService : ISignalRService
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl = "http://localhost:5086/messagehub";

    public event Action<int, int, string, DateTime>? OnMessageReceived;
    public event Action<string>? OnNotificationReceived;
    public event Action<int, int, string>? OnUserTyping;
    public event Action<int, int>? OnMessagesRead;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async Task StartAsync()
    {
        if (_hubConnection != null)
            await _hubConnection.DisposeAsync();

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.UseDefaultCredentials = true;
                options.Credentials = System.Net.CredentialCache.DefaultCredentials;
            })
            .WithAutomaticReconnect(new[]
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(10)
            })
            .Build();

        _hubConnection.On<int, int, string, DateTime>("ReceiveMessage",
            (conversationId, senderId, message, timestamp) =>
                OnMessageReceived?.Invoke(conversationId, senderId, message, timestamp));

        _hubConnection.On<string>("ReceiveNotification",
            message => OnNotificationReceived?.Invoke(message));

        _hubConnection.On<int, int, string>("UserIsTyping",
            (conversationId, userId, userName) =>
                OnUserTyping?.Invoke(conversationId, userId, userName));

        _hubConnection.On<int, int>("MessagesRead",
            (conversationId, userId) =>
                OnMessagesRead?.Invoke(conversationId, userId));

        // Événements de connexion
        _hubConnection.Reconnecting += error =>
        {
            Console.WriteLine($"SignalR reconnecting: {error?.Message}");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine($"SignalR reconnected: {connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            Console.WriteLine($"SignalR connection closed: {error?.Message}");
            return Task.CompletedTask;
        };

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("SignalR connected successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting SignalR: {ex.Message}");
        }
    }

    public async Task JoinConversation(int conversationId)
    {
        if (IsConnected)
            await _hubConnection!.InvokeAsync("JoinConversation", conversationId);
    }

    public async Task LeaveConversation(int conversationId)
    {
        if (IsConnected)
            await _hubConnection!.InvokeAsync("LeaveConversation", conversationId);
    }

    public async Task SendMessage(int conversationId, int senderId, string message)
    {
        if (IsConnected)
            await _hubConnection!.InvokeAsync("SendMessage", conversationId, senderId, message);
    }

    public async Task NotifyTyping(int conversationId, int userId, string userName)
    {
        if (IsConnected)
            await _hubConnection!.InvokeAsync("UserTyping", conversationId, userId, userName);
    }

    public async Task MarkAsRead(int conversationId, int userId)
    {
        if (IsConnected)
            await _hubConnection!.InvokeAsync("MarkAsRead", conversationId, userId);
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}