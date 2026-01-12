using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Text.Json;

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

            var baseUrl = configuration["ApiSettings:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5086";
            _hubUrl = $"{baseUrl}/messagehub";

            Console.WriteLine($"🔧 [SignalR] Hub URL configurée: {_hubUrl}");
        }

        public async Task InitializeAsync()
        {
            if (_hubConnection != null)
            {
                Console.WriteLine($"⚠️ [SignalR] Connexion déjà initialisée (État: {_hubConnection.State})");

                if (_hubConnection.State == HubConnectionState.Connected)
                    return;

                if (_hubConnection.State == HubConnectionState.Connecting)
                {
                    await Task.Delay(1000);
                    return;
                }
            }

            try
            {
                Console.WriteLine($"🔧 [SignalR] Initialisation pour commandes...");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                                           Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
                        options.Headers.Add("X-Requested-With", "XMLHttpRequest");
                        options.SkipNegotiation = false;
                    })
                    .WithAutomaticReconnect(new[] {
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    })
                    .ConfigureLogging(logging =>
                    {
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                    })
                    .Build();

                _hubConnection.Reconnecting += error =>
                {
                    Console.WriteLine($"🔄 [SignalR] Reconnexion en cours... ({error?.Message})");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    Console.WriteLine($"✅ [SignalR] Reconnecté! ID: {connectionId}");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += error =>
                {
                    Console.WriteLine($"❌ [SignalR] Connexion fermée: {error?.Message}");
                    return Task.CompletedTask;
                };

                // ✅ CORRECTION: Utiliser camelCase pour les propriétés JSON
                _hubConnection.On<JsonElement>("CommandeStateChanged", (data) =>
                {
                    try
                    {
                        Console.WriteLine($"📥 [SignalR] CommandeStateChanged reçu");
                        Console.WriteLine($"   Raw JSON: {data.GetRawText()}");

                        // ⚠️ IMPORTANT: Les propriétés sont en camelCase dans le JSON
                        int idCommande = data.GetProperty("idCommande").GetInt32();
                        int newState = data.GetProperty("newState").GetInt32();
                        string stateName = data.GetProperty("stateName").GetString() ?? "État inconnu";

                        Console.WriteLine($"📥 [SignalR] Désérialisé:");
                        Console.WriteLine($"   - IdCommande: {idCommande}");
                        Console.WriteLine($"   - NewState: {newState}");
                        Console.WriteLine($"   - StateName: {stateName}");

                        OnCommandeStateChanged?.Invoke(idCommande, newState, stateName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [SignalR] Erreur traitement CommandeStateChanged: {ex.Message}");
                        Console.WriteLine($"   Stack: {ex.StackTrace}");
                    }
                });

                // ✅ CORRECTION: Idem pour CommandeNotification (camelCase)
                _hubConnection.On<JsonElement>("CommandeNotification", (data) =>
                {
                    try
                    {
                        Console.WriteLine($"📥 [SignalR] CommandeNotification reçue");
                        Console.WriteLine($"   Raw JSON: {data.GetRawText()}");

                        // ⚠️ IMPORTANT: Les propriétés sont en camelCase dans le JSON
                        string message = data.GetProperty("message").GetString() ?? "Notification reçue";
                        int newState = data.GetProperty("newState").GetInt32();

                        Console.WriteLine($"📥 [SignalR] Message: {message}");
                        OnCommandeNotification?.Invoke(message);

                        _notificationService.ShowInfo("Mise à jour de commande", message);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ [SignalR] Erreur traitement CommandeNotification: {ex.Message}");
                        Console.WriteLine($"   Stack: {ex.StackTrace}");
                    }
                });

                Console.WriteLine($"🔧 [SignalR] Démarrage de la connexion vers {_hubUrl}...");
                await _hubConnection.StartAsync();
                Console.WriteLine($"✅ [SignalR] Connexion établie!");
                Console.WriteLine($"   - État: {_hubConnection.State}");
                Console.WriteLine($"   - ID: {_hubConnection.ConnectionId}");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"❌ [SignalR] Erreur HTTP: {httpEx.Message}");
                Console.WriteLine($"   Vérifiez que l'API est démarrée sur {_hubUrl}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SignalR] Erreur: {ex.Message}");
                Console.WriteLine($"   Type: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"   Inner: {ex.InnerException.Message}");
                }
            }
        }

        public async Task JoinCommandeGroup(int idCommande)
        {
            Console.WriteLine($"🔧 [SignalR] Tentative de rejoindre commande_{idCommande}");
            Console.WriteLine($"   État connexion: {_hubConnection?.State}");

            if (_hubConnection?.State == HubConnectionState.Connecting)
            {
                Console.WriteLine("⏳ [SignalR] En attente de connexion...");
                var timeout = DateTime.Now.AddSeconds(5);
                while (_hubConnection.State == HubConnectionState.Connecting && DateTime.Now < timeout)
                {
                    await Task.Delay(100);
                }
            }

            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinCommande", idCommande);
                    Console.WriteLine($"✅ [SignalR] Groupe commande_{idCommande} rejoint!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [SignalR] Erreur JoinCommande: {ex.Message}");

                    try
                    {
                        Console.WriteLine("🔄 [SignalR] Nouvelle tentative...");
                        await Task.Delay(1000);
                        await _hubConnection.InvokeAsync("JoinCommande", idCommande);
                        Console.WriteLine($"✅ [SignalR] Groupe rejoint après nouvelle tentative");
                    }
                    catch (Exception retryEx)
                    {
                        Console.WriteLine($"❌ [SignalR] Échec après retry: {retryEx.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"❌ [SignalR] Impossible de rejoindre - État: {_hubConnection?.State}");

                if (_hubConnection?.State == HubConnectionState.Disconnected)
                {
                    Console.WriteLine("🔄 [SignalR] Tentative de reconnexion...");
                    try
                    {
                        await InitializeAsync();
                        await JoinCommandeGroup(idCommande);
                    }
                    catch (Exception reconnectEx)
                    {
                        Console.WriteLine($"❌ [SignalR] Échec reconnexion: {reconnectEx.Message}");
                    }
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
                    Console.WriteLine($"👋 [SignalR] Quitté le groupe commande_{idCommande}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [SignalR] Erreur LeaveCommande: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    Console.WriteLine("🧹 [SignalR] Nettoyage de la connexion...");
                    await _hubConnection.DisposeAsync();
                    _hubConnection = null;
                    Console.WriteLine("✅ [SignalR] Connexion nettoyée");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [SignalR] Erreur dispose: {ex.Message}");
                }
            }
        }
    }
}