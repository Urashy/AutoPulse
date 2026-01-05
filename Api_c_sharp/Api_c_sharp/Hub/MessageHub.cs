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
                    offreValeur,
                    idAnnonce);
        }
    }
}