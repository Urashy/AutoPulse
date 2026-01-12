using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Api_c_sharp.Hubs
{
    public class MessageHub : Hub
    {
        // Dictionnaire pour stocker les connexions des utilisateurs
        private static readonly ConcurrentDictionary<int, HashSet<string>> UserConnections = new();
        
        // Dictionnaire pour mapper connectionId -> userId
        private static readonly ConcurrentDictionary<string, int> ConnectionUsers = new();

        public override async Task OnConnectedAsync()
        {
            // Récupérer l'ID utilisateur depuis le contexte (cookie JWT)
            var userIdClaim = Context.User?.FindFirst("idUser")?.Value;
            
            if (int.TryParse(userIdClaim, out int userId))
            {
                // Ajouter la connexion
                UserConnections.AddOrUpdate(
                    userId,
                    new HashSet<string> { Context.ConnectionId },
                    (key, existingSet) => 
                    {
                        existingSet.Add(Context.ConnectionId);
                        return existingSet;
                    }
                );
                
                ConnectionUsers[Context.ConnectionId] = userId;
                
                Console.WriteLine($"User {userId} connected with connectionId {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (ConnectionUsers.TryRemove(Context.ConnectionId, out int userId))
            {
                if (UserConnections.TryGetValue(userId, out var connections))
                {
                    connections.Remove(Context.ConnectionId);
                    
                    if (connections.Count == 0)
                    {
                        UserConnections.TryRemove(userId, out _);
                    }
                }
                
                Console.WriteLine($"User {userId} disconnected connectionId {Context.ConnectionId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
        
        // Envoyer un message à une conversation
        public async Task SendMessage(int conversationId, int senderId, string message)
        {
            // Broadcaster à tous les participants de la conversation
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", conversationId, senderId, message, DateTime.UtcNow);
        }

        // Rejoindre une conversation
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            Console.WriteLine($"Connection {Context.ConnectionId} joined conversation {conversationId}");
        }

        // Quitter une conversation
        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
            Console.WriteLine($"Connection {Context.ConnectionId} left conversation {conversationId}");
        }

        // Notifier qu'un utilisateur tape
        public async Task UserTyping(int conversationId, int userId, string userName)
        {
            await Clients.OthersInGroup($"conversation_{conversationId}")
                .SendAsync("UserIsTyping", conversationId, userId, userName);
        }

        // Marquer les messages comme lus
        public async Task MarkAsRead(int conversationId, int userId)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("MessagesRead", conversationId, userId);
        }

        // Obtenir le nombre de connexions actives pour un utilisateur
        public static int GetUserConnectionCount(int userId)
        {
            return UserConnections.TryGetValue(userId, out var connections) 
                ? connections.Count 
                : 0;
        }

        // Envoyer une notification à un utilisateur spécifique
        public static async Task SendNotificationToUser(IHubContext<MessageHub> hubContext, int userId, string message)
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", message);
                }
            }
        }
        
        // Rejoindre le groupe des favoris d'une annonce
        public async Task JoinFavorisAnnonce(int idAnnonce)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"favoris_annonce_{idAnnonce}");
            Console.WriteLine($"Connection {Context.ConnectionId} joined favoris group for annonce {idAnnonce}");
        }

        // Quitter le groupe des favoris d'une annonce
        public async Task LeaveFavorisAnnonce(int idAnnonce)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"favoris_annonce_{idAnnonce}");
            Console.WriteLine($"Connection {Context.ConnectionId} left favoris group for annonce {idAnnonce}");
        }

        // Méthode statique pour notifier une baisse de prix
        public static async Task NotifyPriceDrop(
            IHubContext<MessageHub> hubContext, 
            int idAnnonce, 
            double oldPrice, 
            double newPrice, 
            string annonceLibelle)
        {
            var groupName = $"favoris_annonce_{idAnnonce}";
    
            await hubContext.Clients.Group(groupName)
                .SendAsync("PriceDropNotification", new 
                {
                    IdAnnonce = idAnnonce,
                    AnnonceLibelle = annonceLibelle,
                    OldPrice = oldPrice,
                    NewPrice = newPrice,
                    Reduction = oldPrice - newPrice
                });
    
            Console.WriteLine($"📉 Price drop notification sent to group {groupName}");
        }
        
        public static async Task NotifyNewConversation(
            IHubContext<MessageHub> hubContext,
            int conversationId,
            int senderId,
            int receiverId,
            string firstMessage)
        {
            // Notifier le destinataire
            if (UserConnections.TryGetValue(receiverId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("NewConversationCreated", new
                        {
                            IdConversation = conversationId,
                            SenderId = senderId,
                            FirstMessage = firstMessage,
                            CreatedAt = DateTime.UtcNow
                        });
                }
            }
    
            Console.WriteLine($"💬 New conversation {conversationId} notification sent to user {receiverId}");
        }

        public async Task SendMessageWithOffre(
            int conversationId,
            int senderId,
            string message,
            DateTime dateTime,
            int idMessage,
            int idOffre,
            decimal offreValeur,
            int idAnnonce)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessageWithOffre",
                    conversationId,
                    senderId,
                    message,
                    dateTime,
                    idMessage,
                    idOffre,
                    offreValeur,
                    idAnnonce);
        }
        
        public static async Task SendOffreNotification(
            IHubContext<MessageHub> hubContext,
            int userId,
            int idOffre,
            decimal offreValeur,
            string annonceLibelle)
        {
            if (UserConnections.TryGetValue(userId, out var connections))
            {
                foreach (var connectionId in connections)
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("NewOffreReceived", new
                        {
                            IdOffre = idOffre,
                            Valeur = offreValeur,
                            AnnonceLibelle = annonceLibelle,
                            ReceivedAt = DateTime.UtcNow
                        });
                }
        
                Console.WriteLine($"💰 Notification d'offre envoyée à l'utilisateur {userId} (valeur: {offreValeur}€)");
            }
        }

        // COMMANDES


        // Rejoindre le groupe d'une commande spécifique
        public async Task JoinCommande(int idCommande)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"commande_{idCommande}");
            Console.WriteLine($"Connection {Context.ConnectionId} joined commande {idCommande}");
        }

        // Quitter le groupe d'une commande
        public async Task LeaveCommande(int idCommande)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"commande_{idCommande}");
            Console.WriteLine($"Connection {Context.ConnectionId} left commande {idCommande}");
        }

        // Méthode statique pour notifier un changement d'état de commande
        public static async Task NotifyCommandeStateChanged(
    IHubContext<MessageHub> hubContext,
    int idCommande,
    int newState,
    string stateName,
    int? idAcheteur,
    int? idVendeur)
        {
            var groupName = $"commande_{idCommande}";

            Console.WriteLine($"📦 [Hub] NotifyCommandeStateChanged appelé:");
            Console.WriteLine($"   - Groupe: {groupName}");
            Console.WriteLine($"   - NewState: {newState}");
            Console.WriteLine($"   - StateName: {stateName}");

            var payload = new
            {
                IdCommande = idCommande,
                NewState = newState,
                StateName = stateName,
                Timestamp = DateTime.UtcNow
            };

            Console.WriteLine($"   - Payload JSON: {System.Text.Json.JsonSerializer.Serialize(payload)}");

            await hubContext.Clients.Group(groupName)
                .SendAsync("CommandeStateChanged", payload);

            Console.WriteLine($"   ✅ SendAsync 'CommandeStateChanged' exécuté pour groupe {groupName}");

            // Notification personnalisée pour l'acheteur
            if (idAcheteur.HasValue && UserConnections.TryGetValue(idAcheteur.Value, out var acheteurConnections))
            {
                Console.WriteLine($"   📨 Envoi notification à acheteur {idAcheteur.Value} ({acheteurConnections.Count} connexions)");
                foreach (var connectionId in acheteurConnections)
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("CommandeNotification", new
                        {
                            IdCommande = idCommande,
                            Message = GetNotificationMessageForBuyer(newState),
                            NewState = newState,
                            Type = "commande_update"
                        });
                }
            }

            // Notification personnalisée pour le vendeur
            if (idVendeur.HasValue && UserConnections.TryGetValue(idVendeur.Value, out var vendeurConnections))
            {
                Console.WriteLine($"   📨 Envoi notification à vendeur {idVendeur.Value} ({vendeurConnections.Count} connexions)");
                foreach (var connectionId in vendeurConnections)
                {
                    await hubContext.Clients.Client(connectionId)
                        .SendAsync("CommandeNotification", new
                        {
                            IdCommande = idCommande,
                            Message = GetNotificationMessageForSeller(newState),
                            NewState = newState,
                            Type = "commande_update"
                        });
                }
            }

            Console.WriteLine($"📦 [Hub] Notifications envoyées pour commande {idCommande}");
        }

        // Messages pour l'acheteur
        private static string GetNotificationMessageForBuyer(int state)
        {
            return state switch
            {
                2 => "Le vendeur a été notifié de votre paiement",
                3 => "Le vendeur a confirmé la réception de votre paiement",
                4 => "Le vendeur a émis la livraison du véhicule",
                5 => "Transaction terminée ! Profitez de votre véhicule",
                _ => "Statut de la commande mis à jour"
            };
        }

        // Messages pour le vendeur
        private static string GetNotificationMessageForSeller(int state)
        {
            return state switch
            {
                2 => "L'acheteur a déclaré un paiement",
                3 => "Vous avez confirmé la réception du paiement",
                4 => "Vous avez émis la livraison",
                5 => "L'acheteur a confirmé la réception du véhicule",
                _ => "Statut de la commande mis à jour"
            };
        }
    }
}