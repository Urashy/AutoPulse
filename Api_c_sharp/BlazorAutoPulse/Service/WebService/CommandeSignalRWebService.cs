using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace BlazorAutoPulse.Service.WebService
{
    public class CommandeSignalRWebService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NotificationService _notificationService;
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public event Action<int, int, string>? OnCommandeStateChanged;
        public event Action<string>? OnCommandeNotification;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public CommandeSignalRWebService(
            IJSRuntime jsRuntime,
            NotificationService notificationService,
            IConfiguration configuration)
        {
            _jsRuntime = jsRuntime;
            _notificationService = notificationService;
            _hubUrl = configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') + "/hubs/message"
                      ?? "https://localhost:7272/hubs/message";
        }

        public async Task InitializeAsync()
        {
            if (_hubConnection != null)
                return;

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.UseDefaultCredentials = true;
                    })
                    .WithAutomaticReconnect()
                    .Build();

                // Écouter les changements d'état
                _hubConnection.On<dynamic>("CommandeStateChanged", (data) =>
                {
                    int idCommande = (int)data.IdCommande;
                    int newState = (int)data.NewState;
                    string stateName = (string)data.StateName;

                    Console.WriteLine($"[SignalR] Commande {idCommande} -> État {newState}");
                    OnCommandeStateChanged?.Invoke(idCommande, newState, stateName);
                });

                // Écouter les notifications
                _hubConnection.On<dynamic>("CommandeNotification", (data) =>
                {
                    string message = (string)data.Message;
                    int newState = (int)data.NewState;

                    Console.WriteLine($"[SignalR] Notification: {message}");
                    OnCommandeNotification?.Invoke(message);

                    // Afficher une notification visuelle
                    _notificationService.ShowInfo("Mise à jour de commande", message);
                });

                await _hubConnection.StartAsync();
                Console.WriteLine("[SignalR] Connexion établie pour les commandes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] Erreur connexion: {ex.Message}");
            }
        }

        public async Task JoinCommandeGroup(int idCommande)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinCommande", idCommande);
                    Console.WriteLine($"[SignalR] Rejoint le groupe commande_{idCommande}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Erreur JoinCommande: {ex.Message}");
                }
            }
        }

        public async Task LeaveCommandeGroup(int idCommande)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("LeaveCommande", idCommande);
                    Console.WriteLine($"[SignalR] Quitté le groupe commande_{idCommande}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SignalR] Erreur LeaveCommande: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}