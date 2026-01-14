using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorAutoPulse.Service.WebService;

public class SignalRWebService : ISignalRService, IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly string _hubUrl;

    public event Action<int, int, string, DateTime>? OnMessageReceived;
    public event Action<int, int, string>? OnUserTyping;
    public event Action<int, int>? OnMessagesRead;
    public event Action<PriceDropNotification>? OnPriceDropReceived;
    public event Action<int, bool?>? OnOffreStatusChanged;
    public event Action<int, int, string, DateTime, int,int, decimal, int>? OnMessageWithOffreReceived;
    public event Action<OffreNotification>? OnOffreReceived;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public SignalRWebService()
    {
        //"http://localhost:5086/messagehub";/
        _hubUrl = "https://api-autopulse-d8hgfvgjbsapataf.francecentral-01.azurewebsites.net/messagehub";

        Console.WriteLine("[SignalR] Service initialized with hub URL: " + _hubUrl);
    }

    public async Task StartAsync()
    {
        Console.WriteLine("[SignalR] StartAsync called");

        if (_hubConnection != null && IsConnected)
        {
            Console.WriteLine("[SignalR] Already connected — ignoring StartAsync()");
            return;
        }

        Console.WriteLine("[SignalR] Building HubConnection...");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(_hubUrl, options =>
            {
                options.DefaultTransferFormat = Microsoft.AspNetCore.Connections.TransferFormat.Text;
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

        _hubConnection.Reconnecting += error =>
        {
            Console.WriteLine("[SignalR] Reconnecting... Error: " + error?.Message);
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            Console.WriteLine("[SignalR] Reconnected. New connection ID: " + connectionId);
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            Console.WriteLine("[SignalR] Connection closed. Error: " + error?.Message);
            return Task.CompletedTask;
        };

        // Écouter les messages reçus
        _hubConnection.On<int, int, string, DateTime>("ReceiveMessage",
            (conversationId, senderId, message, date) =>
            {
                Console.WriteLine($"[SignalR] ReceiveMessage: conv={conversationId}, sender={senderId}, msg={message}");
                OnMessageReceived?.Invoke(conversationId, senderId, message, date);
            });

        // Écouter les changements de statut des offres
        _hubConnection.On<int, bool?>("OffreStatusChanged", (idOffre, estAccepte) =>
        {
            Console.WriteLine($"[SignalR] Offre {idOffre} status changed: {estAccepte}");
            OnOffreStatusChanged?.Invoke(idOffre, estAccepte);
        });

        _hubConnection.On<int, int, string, DateTime, int, int, decimal, int>("ReceiveMessageWithOffre",
            (conversationId, senderId, message, date, idMessage, idOffre, offreValeur, idAnnonce) =>
            {
                Console.WriteLine($"[SignalR] ReceiveMessageWithOffre: conv={conversationId}, offre={offreValeur}€, idOffre={idOffre}");
                OnMessageWithOffreReceived?.Invoke(conversationId, senderId, message, date, idMessage, idOffre, offreValeur, idAnnonce);
            });

        // Écouter les notifications de frappe
        _hubConnection.On<int, int, string>("UserIsTyping",
            (conversationId, userId, userName) =>
            {
                Console.WriteLine($"[SignalR] UserIsTyping: conv={conversationId}, user={userId}, name={userName}");
                OnUserTyping?.Invoke(conversationId, userId, userName);
            });

        // Écouter les messages lus
        _hubConnection.On<int, int>("MessagesRead",
            (conversationId, userId) =>
            {
                Console.WriteLine($"[SignalR] MessagesRead: conv={conversationId}, user={userId}");
                OnMessagesRead?.Invoke(conversationId, userId);
            });
        
        _hubConnection.On<PriceDropNotification>("PriceDropNotification", notification =>
        {
            Console.WriteLine($"[SignalR] Price drop received for annonce {notification.IdAnnonce}");
            OnPriceDropReceived?.Invoke(notification);
        });
        
        _hubConnection.On<object>("NewOffreReceived", (data) =>
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var notification = System.Text.Json.JsonSerializer.Deserialize<OffreNotification>(json);
                Console.WriteLine($"[SignalR] NewOffre received: {notification.IdOffre}");
        
                if (notification != null)
                {
                    OnOffreReceived?.Invoke(notification);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur désérialisation offre: {ex.Message}");
            }
        });

        Console.WriteLine("[SignalR] Starting connection...");

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("[SignalR] Connected successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[SignalR] ERROR while starting connection: " + ex.Message);
            throw;
        }
    }

    public async Task StopAsync()
    {
        Console.WriteLine("[SignalR] StopAsync called");

        if (_hubConnection != null)
        {
            try
            {
                await _hubConnection.StopAsync();
                Console.WriteLine("[SignalR] Connection stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SignalR] Error stopping connection: " + ex.Message);
            }

            try
            {
                await _hubConnection.DisposeAsync();
                Console.WriteLine("[SignalR] HubConnection disposed");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SignalR] Error disposing: " + ex.Message);
            }

            _hubConnection = null;
        }
    }

    public async Task JoinConversation(int conversationId)
    {
        Console.WriteLine($"[SignalR] Joining conversation {conversationId}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("JoinConversation", conversationId);
            Console.WriteLine($"[SignalR] Joined conversation: {conversationId}");
        }
        else
        {
            Console.WriteLine("[SignalR] Cannot join conversation: not connected.");
        }
    }

    public async Task LeaveConversation(int conversationId)
    {
        Console.WriteLine($"[SignalR] Leaving conversation {conversationId}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("LeaveConversation", conversationId);
            Console.WriteLine($"[SignalR] Left conversation: {conversationId}");
        }
        else
        {
            Console.WriteLine("[SignalR] Cannot leave conversation: not connected.");
        }
    }

    public async Task SendMessage(int conversationId, int senderId, string message)
    {
        Console.WriteLine($"[SignalR] Sending message in conv={conversationId}, sender={senderId}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("SendMessage", conversationId, senderId, message);
            Console.WriteLine("[SignalR] Message sent");
        }
        else
        {
            Console.WriteLine("[SignalR] Cannot send message: not connected.");
        }
    }

    public async Task SendMessageWithOffre(int conversationId, int senderId, string message, int idMessage, int idOffre, decimal offreValeur, int idAnnonce)
    {
        Console.WriteLine($"[SignalR] Sending message with offre: {offreValeur}€, idOffre={idOffre}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("SendMessageWithOffre",
                conversationId, senderId, message, DateTime.UtcNow, idMessage, idOffre, offreValeur, idAnnonce);
        }
    }
    public async Task NotifyTyping(int conversationId, int userId, string userName)
    {
        Console.WriteLine($"[SignalR] NotifyTyping: conv={conversationId}, user={userId}, name={userName}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("UserTyping", conversationId, userId, userName);
        }
        else
        {
            Console.WriteLine("[SignalR] Cannot notify typing: not connected.");
        }
    }

    public async Task MarkAsRead(int conversationId, int userId)
    {
        Console.WriteLine($"[SignalR] MarkAsRead: conv={conversationId}, user={userId}");

        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("MarkAsRead", conversationId, userId);
        }
        else
        {
            Console.WriteLine("[SignalR] Cannot mark messages as read: not connected.");
        }
    }
    
    // Rejoindre le groupe favoris d'une annonce
    public async Task JoinFavorisAnnonce(int idAnnonce)
    {
        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("JoinFavorisAnnonce", idAnnonce);
            Console.WriteLine($"[SignalR] Joined favoris group for annonce: {idAnnonce}");
        }
    }

    // Quitter le groupe favoris d'une annonce
    public async Task LeaveFavorisAnnonce(int idAnnonce)
    {
        if (_hubConnection != null && IsConnected)
        {
            await _hubConnection.InvokeAsync("LeaveFavorisAnnonce", idAnnonce);
            Console.WriteLine($"[SignalR] Left favoris group for annonce: {idAnnonce}");
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        Console.WriteLine("[SignalR] DisposeAsync called");
        await StopAsync();
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
        Console.WriteLine("[SignalR] Fully disposed");
    }
}