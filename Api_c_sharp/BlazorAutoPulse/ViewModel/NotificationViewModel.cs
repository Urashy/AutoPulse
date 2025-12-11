using AutoPulse.Shared.DTO;
using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service.Interface;
using BlazorAutoPulse.Service.WebService;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class NotificationViewModel : IDisposable
    {
        private readonly INotificationService _notificationService;
        private readonly ICompteService _compteService;
        private readonly ISignalRService _signalRService;
        private readonly NavigationManager _navigation;

        public List<NotificationDTO> Notifications { get; private set; } = new();
        public int UnreadCount { get; private set; } = 0;
        public bool IsLoading { get; private set; } = true;
        public bool ShowOnlyUnread { get; set; } = false;
        
        private int _currentUserId;
        private Action? _refreshUI;
        
        public event Action? OnNotificationCountChanged;

        public NotificationViewModel(
            INotificationService notificationService,
            ICompteService compteService,
            ISignalRService signalRService,
            NavigationManager navigation)
        {
            _notificationService = notificationService;
            _compteService = compteService;
            _signalRService = signalRService;
            _navigation = navigation;
        }

        public async Task InitializeAsync(Action refreshUI)
        {
            _refreshUI = refreshUI;
            try
            {
                var compte = await _compteService.GetMe();
                _currentUserId = compte.IdCompte;
                _signalRService.OnPriceDropReceived += HandlePriceDropNotification;
            
                await LoadNotifications();
            }
            catch
            {
                _navigation.NavigateTo("/connexion");
            }
        }

        public async Task LoadNotifications()
        {
            IsLoading = true;
            _refreshUI?.Invoke();

            try
            {
                if (ShowOnlyUnread)
                {
                    Notifications = (await _notificationService.GetUnreadNotificationsByCompteAsync(_currentUserId)).ToList();
                }
                else
                {
                    Notifications = (await _notificationService.GetNotificationsByCompteAsync(_currentUserId)).ToList();
                }
                
                UnreadCount = await _notificationService.GetUnreadCountAsync(_currentUserId);
                
                Console.WriteLine($"📬 {Notifications.Count} notifications chargées ({UnreadCount} non lues)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur chargement notifications: {ex.Message}");
                Notifications = new List<NotificationDTO>();
            }
            finally
            {
                IsLoading = false;
                _refreshUI?.Invoke();
            }
        }

        public async Task MarkAsRead(NotificationDTO notification)
        {
            if (notification.EstLue) return;

            try
            {
                await _notificationService.MarkAsReadAsync(notification.IdNotification);
                notification.EstLue = true;
                UnreadCount = Math.Max(0, UnreadCount - 1);
                OnNotificationCountChanged?.Invoke();
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur marquage notification lue: {ex.Message}");
            }
        }

        public async Task MarkAllAsRead()
        {
            try
            {
                await _notificationService.MarkAllAsReadAsync(_currentUserId);
                
                foreach (var notification in Notifications)
                {
                    notification.EstLue = true;
                }
                
                UnreadCount = 0;
                OnNotificationCountChanged?.Invoke();
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur marquage toutes notifications lues: {ex.Message}");
            }
        }

        public async Task DeleteNotification(NotificationDTO notification)
        {
            try
            {
                await _notificationService.DeleteAsync(notification.IdNotification);
                Notifications.Remove(notification);
                
                if (!notification.EstLue)
                {
                    UnreadCount = Math.Max(0, UnreadCount - 1);
                }
                
                OnNotificationCountChanged?.Invoke();
                _refreshUI?.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur suppression notification: {ex.Message}");
            }
        }

        public void NavigateToNotification(NotificationDTO notification)
        {
            _ = MarkAsRead(notification);
            
            if (!string.IsNullOrEmpty(notification.UrlNavigation))
            {
                _navigation.NavigateTo(notification.UrlNavigation);
            }
        }

        public async Task ToggleFilter()
        {
            ShowOnlyUnread = !ShowOnlyUnread;
            await LoadNotifications();
        }

        private void HandlePriceDropNotification(PriceDropNotification priceDropData)
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);
                await LoadNotifications();
                OnNotificationCountChanged?.Invoke();
            });
        }

        public string GetNotificationIcon(string type)
        {
            return type switch
            {
                "pricedrop" => "💰",
                "info" => "ℹ️",
                "success" => "✅",
                "warning" => "⚠️",
                "error" => "❌",
                _ => "🔔"
            };
        }

        public string GetRelativeTime(DateTime date)
        {
            var diff = DateTime.UtcNow - date;
            
            if (diff.TotalMinutes < 1) return "À l'instant";
            if (diff.TotalMinutes < 60) return $"Il y a {(int)diff.TotalMinutes} min";
            if (diff.TotalHours < 24) return $"Il y a {(int)diff.TotalHours}h";
            if (diff.TotalDays < 7) return $"Il y a {(int)diff.TotalDays}j";
            
            return date.ToString("dd/MM/yyyy");
        }

        public void Dispose()
        {
            _signalRService.OnPriceDropReceived -= HandlePriceDropNotification;
        }
    }
}