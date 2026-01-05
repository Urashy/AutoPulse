using BlazorAutoPulse.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorAutoPulse.Service.WebService;

public interface ISignalRService
{
    Task StartAsync();
    Task StopAsync();
    Task JoinConversation(int conversationId);
    Task LeaveConversation(int conversationId);
    Task SendMessage(int conversationId, int senderId, string message);
    Task NotifyTyping(int conversationId, int userId, string userName);
    Task MarkAsRead(int conversationId, int userId);
    Task JoinFavorisAnnonce(int idAnnonce);
    Task LeaveFavorisAnnonce(int idAnnonce);

    event Action<int, int, string, DateTime>? OnMessageReceived;
    event Action<int, int, string>? OnUserTyping;
    event Action<int, int>? OnMessagesRead;
    public event Action<PriceDropNotification>? OnPriceDropReceived;
    public event Action<int, bool?>? OnOffreStatusChanged;

    bool IsConnected { get; }
}