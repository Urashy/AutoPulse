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
    
    // Événements pour notifier le ViewModel
    event Action<int, int, string, DateTime>? OnMessageReceived;
    event Action<int, int, string>? OnUserTyping;
    event Action<int, int>? OnMessagesRead;
}