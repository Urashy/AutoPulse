using BlazorAutoPulse.Model;

namespace BlazorAutoPulse.Service
{
    /// <summary>
    /// Service de notifications toast utilisant le pattern Singleton et Observer
    /// </summary>
    public class NotificationService
    {
        // Pattern Observer : événement pour notifier les composants
        public event Action<ToastNotification>? OnNotificationReceived;
        
        // ID de l'utilisateur courant pour filtrer ses propres messages
        private int? _currentUserId;
        
        public void SetCurrentUserId(int userId)
        {
            _currentUserId = userId;
        }
        
        /// <summary>
        /// Affiche une notification de succès
        /// </summary>
        public void ShowSuccess(string title, string message, string? navigationUrl = null)
        {
            Show(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = NotificationType.Success,
                NavigationUrl = navigationUrl,
            });
        }
        
        /// <summary>
        /// Affiche une notification d'information
        /// </summary>
        public void ShowInfo(string title, string message, string? navigationUrl = null)
        {
            Show(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = NotificationType.Info,
                NavigationUrl = navigationUrl
            });
        }
        
        /// <summary>
        /// Affiche une notification d'avertissement
        /// </summary>
        public void ShowWarning(string title, string message, string? navigationUrl = null)
        {
            Show(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = NotificationType.Warning,
                NavigationUrl = navigationUrl
            });
        }
        
        /// <summary>
        /// Affiche une notification d'erreur
        /// </summary>
        public void ShowError(string title, string message, string? navigationUrl = null)
        {
            Show(new ToastNotification
            {
                Title = title,
                Message = message,
                Type = NotificationType.Error,
                NavigationUrl = navigationUrl
            });
        }
        
        /// <summary>
        /// Affiche une notification de nouveau message
        /// </summary>
        public void ShowNewMessage(string senderName, string messagePreview, int? senderId = null)
        {
            // Ne pas afficher si c'est l'utilisateur courant qui a envoyé
            if (senderId.HasValue && _currentUserId.HasValue && senderId.Value == _currentUserId.Value)
            {
                return;
            }
            
            Show(new ToastNotification
            {
                Title = senderName,
                Message = TruncateMessage(messagePreview, 40),
                Type = NotificationType.Message,
                NavigationUrl = "/conversations",
                DurationMs = 6000,
            });
        }
        
        /// <summary>
        /// Pattern Builder : méthode privée pour créer et déclencher une notification
        /// </summary>
        private void Show(ToastNotification notification)
        {
            notification.Id = Guid.NewGuid();
            notification.CreatedAt = DateTime.Now;
            OnNotificationReceived?.Invoke(notification);
        }
        
        /// <summary>
        /// Tronque le message à une longueur maximale
        /// </summary>
        private string TruncateMessage(string message, int maxLength)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            
            return message.Length <= maxLength 
                ? message 
                : message.Substring(0, maxLength) + "...";
        }
    }
}