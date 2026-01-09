using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Models.Repository.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<Notification>> GetNotificationsByCompteAsync(int idCompte);
    Task<IEnumerable<Notification>> GetUnreadNotificationsByCompteAsync(int idCompte);
    Task<int> GetUnreadCountAsync(int idCompte);
    Task MarkAsReadAsync(int idNotification);
    Task MarkAllAsReadAsync(int idCompte);
    Task DeleteOldNotificationsAsync(int daysOld = 30);
    Task NotifAnnonce(int idannonce, double prixold, double prixnew);
    Task NotifSuppressionAnnonce(int idannonce);
    Task NotifOffreAnnonce(int idannonce, int idAcheteur, decimal valeur);
    Task NotifPaiementMiseEnAvant(int idannonce, int idCompte, int idMiseEnAvant);
}