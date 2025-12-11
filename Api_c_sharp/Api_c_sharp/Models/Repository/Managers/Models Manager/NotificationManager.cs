using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class NotificationManager : WriteableReadableManager<Notification>, INotificationService
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

        public virtual async Task MarkAsReadAsync(int idNotification)
        {
            var notification = await dbSet.FindAsync(idNotification);
            if (notification == null);

            notification.EstLue = true;
            await context.SaveChangesAsync();
        }

        public virtual async Task MarkAllAsReadAsync(int idCompte)
        {
            var notifications = await dbSet
                .Where(n => n.IdCompte == idCompte && !n.EstLue)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.EstLue = true;
            }

            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteOldNotificationsAsync(int daysOld = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var oldNotifications = await dbSet
                .Where(n => n.DateCreation < cutoffDate && n.EstLue)
                .ToListAsync();

            dbSet.RemoveRange(oldNotifications);
            await context.SaveChangesAsync();
        }

        public virtual async Task NotifCreationAutoAsync(List<int> idcomptes, string url,string titre,string message,int idannonce, string type, double prix = 0, double prixnew = 0)
        {
            foreach (var idcompte in idcomptes)
            {
                Notification notif = new Notification
                {
                    IdCompte = idcompte,
                    Titre = titre,
                    Message = message,
                    Type = type,
                    UrlNavigation = url,
                    EstLue = false,
                    DateCreation = DateTime.UtcNow,
                    IdAnnonce = idannonce,
                    AncienPrix = prix == 0 ? null : prix,
                    NouveauPrix = prixnew == 0 ? null : prixnew
                };

                await AddAsync(notif);
            }
        }

        public virtual async Task NotifAnnonce(int idannonce, double prixold, double prixnew)
        {
            List<int> idcomptes = await context.Favoris
                .Where(f => f.IdAnnonce == idannonce)
                .Select(f => f.IdCompte)
                .Distinct()
                .ToListAsync();

            string url = $"/annonce/{idannonce}";
            string titre = "Mise � jour de l'annonce";
            string message = $"Le prix de l'annonce #{idannonce} a �t� modifi� de {prixold} � {prixnew}.";
            string type = "information";

            await NotifCreationAutoAsync(idcomptes, url, titre, message, idannonce, type, prixold, prixnew);
        }
    }
}