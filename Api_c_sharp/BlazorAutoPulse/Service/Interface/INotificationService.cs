using AutoPulse.Shared.DTO;

namespace BlazorAutoPulse.Service.Interface;

public interface INotificationService : IService<NotificationDTO>
{
    Task<IEnumerable<NotificationDTO>> GetNotificationsByCompteAsync(int idCompte);
    Task<IEnumerable<NotificationDTO>> GetUnreadNotificationsByCompteAsync(int idCompte);
    Task<int> GetUnreadCountAsync(int idCompte);
    Task MarkAsReadAsync(int idNotification);
    Task MarkAllAsReadAsync(int idCompte);
}