using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using System.Net;

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
                Console.WriteLine($"🔧 [SignalR] Initialisation pour commandes...");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        // ✅ Pour Blazor WASM, utiliser les cookies HTTP
                        options.Credentials = CredentialCache.DefaultCredentials;
                        options.UseStatefulReconnect = true;

                        // ✅ IMPORTANT : Permettre l'envoi des cookies
                        options.HttpMessageHandlerFactory = (handler) =>
                        {
                            if (handler is HttpClientHandler clientHandler)
                            {
                                clientHandler.UseCookies = true;
                                clientHandler.CookieContainer = new CookieContainer();
                            }
                            return handler;
                        };
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5) })
                    .Build();

                // Écouter les changements d'état
                _hubConnection.On<dynamic>("CommandeStateChanged", (data) =>
                {
                    int idCommande = (int)data.IdCommande;
                    int newState = (int)data.NewState;
                    string stateName = (string)data.StateName;

                    Console.WriteLine($"📥 [SignalR] Commande {idCommande} -> État {newState} ({stateName})");
                    OnCommandeStateChanged?.Invoke(idCommande, newState, stateName);
                });

                // Écouter les notifications
                _hubConnection.On<dynamic>("CommandeNotification", (data) =>
                {
                    string message = (string)data.Message;
                    int newState = (int)data.NewState;

                    Console.WriteLine($"📥 [SignalR] Notification: {message}");
                    OnCommandeNotification?.Invoke(message);

                    // Afficher une notification visuelle
                    _notificationService.ShowInfo("Mise à jour de commande", message);
                });

                Console.WriteLine($"🔧 [SignalR] Démarrage de la connexion...");
                await _hubConnection.StartAsync();
                Console.WriteLine($"✅ [SignalR] Connexion établie (État: {_hubConnection.State})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [SignalR] Erreur connexion: {ex.Message}");
                Console.WriteLine($"❌ [SignalR] Type: {ex.GetType().Name}");
                Console.WriteLine($"❌ [SignalR] Stack: {ex.StackTrace}");
            }
        }

        public async Task JoinCommandeGroup(int idCommande)
        {
            Console.WriteLine($"🔧 [SignalR] Tentative de rejoindre commande_{idCommande}");
            Console.WriteLine($"🔧 [SignalR] État connexion: {_hubConnection?.State}");

            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("JoinCommande", idCommande);
                    Console.WriteLine($"✅ [SignalR] Groupe commande_{idCommande} rejoint avec succès");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ [SignalR] Erreur JoinCommande: {ex.Message}");
                    Console.WriteLine($"❌ [SignalR] Stack trace: {ex.StackTrace}");
                }
            }
            else
            {
                Console.WriteLine($"❌ [SignalR] Impossible de rejoindre le groupe - État: {_hubConnection?.State}");
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