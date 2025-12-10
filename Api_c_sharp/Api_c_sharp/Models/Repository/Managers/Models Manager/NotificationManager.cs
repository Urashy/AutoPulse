using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class NotificationManager : WriteableReadableManager<Notification>, INotificationRepository
    {
        public NotificationManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Notification>> GetAllAsync()
        {
            return await dbSet
                .Include(n => n.AnnonceNotificationNav)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<Notification>> GetNotificationsByCompteAsync(int idCompte)
        {
            return await dbSet
                .Include(n => n.AnnonceNotificationNav)
                .Where(n => n.IdCompte == idCompte)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<Notification>> GetUnreadNotificationsByCompteAsync(int idCompte)
        {
            return await dbSet
                .Include(n => n.AnnonceNotificationNav)
                .Where(n => n.IdCompte == idCompte && !n.EstLue)
                .OrderByDescending(n => n.DateCreation)
                .ToListAsync();
        }

        public virtual async Task<int> GetUnreadCountAsync(int idCompte)
        {
            return await dbSet
                .Where(n => n.IdCompte == idCompte && !n.EstLue)
                .CountAsync();
        }

        public virtual async Task<bool> MarkAsReadAsync(int idNotification)
        {
            var notification = await dbSet.FindAsync(idNotification);
            if (notification == null) return false;

            notification.EstLue = true;
            await context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> MarkAllAsReadAsync(int idCompte)
        {
            var notifications = await dbSet
                .Where(n => n.IdCompte == idCompte && !n.EstLue)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.EstLue = true;
            }

            await context.SaveChangesAsync();
            return true;
        }

        public virtual async Task<bool> DeleteOldNotificationsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldNotifications = await dbSet
                .Where(n => n.DateCreation < cutoffDate && n.EstLue)
                .ToListAsync();

            dbSet.RemoveRange(oldNotifications);
            await context.SaveChangesAsync();
            return true;
        }
    }
}