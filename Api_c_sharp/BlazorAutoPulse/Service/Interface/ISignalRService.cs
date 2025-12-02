namespace BlazorAutoPulse.Service.Interface;
public interface ISignalRService : IAsyncDisposable
{
    event Action<int, int, string, DateTime>? OnMessageReceived;
    event Action<string>? OnNotificationReceived;
    event Action<int, int, string>? OnUserTyping;
    event Action<int, int>? OnMessagesRead;

    bool IsConnected { get; }

    Task StartAsync();
    Task StopAsync();

    Task JoinConversation(int conversationId);
    Task LeaveConversation(int conversationId);

    Task SendMessage(int conversationId, int senderId, string message);
    Task NotifyTyping(int conversationId, int userId, string userName);
    Task MarkAsRead(int conversationId, int userId);
}