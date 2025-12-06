using BlazorAutoPulse.Model;
using BlazorAutoPulse.Service;
using Microsoft.AspNetCore.Components;

namespace BlazorAutoPulse.ViewModel
{
    public class ToastViewModel : IDisposable
    {
        private readonly NotificationService _notificationService;
        private readonly NavigationManager _navigation;

        private Action? _refreshUI;

        public List<ToastNotification> ActiveNotifications { get; private set; } = new();
        private Dictionary<Guid, string> AnimationStates { get; set; } = new();

        public ToastViewModel(NotificationService notificationService,
                              NavigationManager navigation)
        {
            _notificationService = notificationService;
            _navigation = navigation;
        }

        // 🔗 Connexion au Razor
        public void Attach(Action refreshUI)
        {
            _refreshUI = refreshUI;
            _notificationService.OnNotificationReceived += HandleNotification;
        }

        public void Detach()
        {
            _notificationService.OnNotificationReceived -= HandleNotification;
        }

        // 🔔 Réception des notifications
        private async void HandleNotification(ToastNotification notification)
        {
            ActiveNotifications.Add(notification);
            AnimationStates[notification.Id] = "show";
            _refreshUI?.Invoke();

            _ = Task.Run(async () =>
            {
                await Task.Delay(notification.DurationMs);
                await Remove(notification);
            });
        }

        // 🖱️ Clic sur la notification (navigation)
        public void HandleClick(ToastNotification notification)
        {
            if (!string.IsNullOrEmpty(notification.NavigationUrl))
            {
                _navigation.NavigateTo(notification.NavigationUrl);
                _ = Remove(notification);
            }
        }

        // ❌ Suppression de toast
        public async Task RemoveNotification(ToastNotification notification)
        {
            await Remove(notification);
        }

        private async Task Remove(ToastNotification notification)
        {
            if (!AnimationStates.ContainsKey(notification.Id))
                return;

            AnimationStates[notification.Id] = "hide";
            _refreshUI?.Invoke();

            await Task.Delay(300);

            ActiveNotifications.Remove(notification);
            AnimationStates.Remove(notification.Id);

            _refreshUI?.Invoke();
        }

        // 🎬 Animations
        public string GetAnimationClass(ToastNotification notification)
        {
            return AnimationStates.TryGetValue(notification.Id, out var state) ? state : "";
        }

        // 🔣 Icônes
        public string GetIcon(NotificationType type)
        {
            return type switch
            {
                NotificationType.Success => "✓",
                NotificationType.Info => "ℹ",
                NotificationType.Warning => "⚠",
                NotificationType.Error => "✕",
                NotificationType.Message => "💬",
                _ => "ℹ"
            };
        }

        public void Dispose() => Detach();
    }
}