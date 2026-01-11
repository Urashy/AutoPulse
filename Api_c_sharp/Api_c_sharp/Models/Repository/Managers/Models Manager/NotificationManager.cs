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
            if (notification == null)
                return;

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

        public virtual async Task NotifCreationAutoAsync(List<int> idcomptes, string url,string titre,string message,int idannonce, string type, double prix = 0, double prixnew = 0, string pseudoAcheteurOffre = "", decimal valeurOffre = 0)
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
                    NouveauPrix = prixnew == 0 ? null : prixnew,
                    PseudoAcheteurOffre = pseudoAcheteurOffre,
                    ValeurOffre = (double)valeurOffre
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
            
            Annonce? annonce = await context.Annonces.FirstOrDefaultAsync(a => idannonce == a.IdAnnonce);

            string url = $"/annonce/{idannonce}";
            string titre = "Mise à jour de l'annonce";
            string message = $"Le prix de l'annonce #{annonce.Libelle} a été modifié de {prixold} à {prixnew}.";
            string type = "pricedrop";

            await NotifCreationAutoAsync(idcomptes, url, titre, message, idannonce, type, prixold, prixnew);
        }

        public virtual async Task NotifSuppressionAnnonce(int idannonce)
        {
            List <int> idcomptes = await context.Annonces
                .Where(a => a.IdAnnonce == idannonce)
                .Select(a => a.IdCompte)
                .ToListAsync();

            Annonce? annonce = await context.Annonces
                .FirstOrDefaultAsync(a => a.IdAnnonce == idannonce);

            Signalement? signalement = await context.Signalements.FirstOrDefaultAsync(s => s.IdAnnonceSignale == annonce.IdAnnonce);

            string url = $"/annonces";
            string titre = "Annonce supprimée";
            string raison = signalement?.DescriptionSignalement ?? "Raison non spécifiée";
            string message = $"Votre annonce a été supprimée par un modérateur : {annonce.Libelle}\nPour la raison: '{raison}'";
            string type = "error";
            await NotifCreationAutoAsync(idcomptes, url, titre, message, idannonce, type);
        }
        
        public virtual async Task NotifOffreAnnonce(int idannonce, int idAcheteur, decimal valeur)
        {
            List <int> idcomptes = await context.Annonces
                .Where(a => a.IdAnnonce == idannonce)
                .Select(a => a.IdCompte)
                .ToListAsync();

            Annonce? annonce = await context.Annonces
                .FirstOrDefaultAsync(a => a.IdAnnonce == idannonce);
            
            Compte compte = await context.Comptes.FindAsync(idAcheteur);

            string url = $"/annonce/{idannonce}";
            string titre = "Offre sur une annonce";
            string message = $"On vous a fait une offre sur votre annonce #{annonce.Libelle}";
            string type = "offre";
            await NotifCreationAutoAsync(idcomptes, url, titre, message, idannonce, type, pseudoAcheteurOffre: compte.Pseudo, valeurOffre: valeur);
        }
        
        public virtual async Task NotifPaiementMiseEnAvant(int idannonce, int idCompte, int idMiseEnAvant)
        {
            List <int> idcomptes = await context.Annonces
                .Where(a => a.IdAnnonce == idannonce)
                .Select(a => a.IdCompte)
                .ToListAsync();

            Annonce? annonce = await context.Annonces
                .FirstOrDefaultAsync(a => a.IdAnnonce == idannonce);
            
            MiseEnAvant miseEnAvant = await context.MisesEnAvant.FindAsync(idMiseEnAvant);

            string url = $"/annonce/{idannonce}";
            string titre = "Paiemement";
            string message = $"Votre mise en avant de grade '{miseEnAvant.LibelleMiseEnAvant} à {miseEnAvant.PrixSemaine}€' pour l'annonce #{annonce.Libelle} à été renouvellé. \nLe changement de grade de la mise en avant se trouve sur la page de modification d'annonce (changement prix en compte la semaine suivant).";
            string type = "paiement";
            await NotifCreationAutoAsync(idcomptes, url, titre, message, idannonce, type);
        }
        
        public virtual async Task NotifOfrreAccepterOuRejeter(int idannonce, int idCompte, int idCommande, decimal montant, bool estAccepte)
        {

            Annonce? annonce = await context.Annonces
                .FirstOrDefaultAsync(a => a.IdAnnonce == idannonce);
            
            Commande? commande = await context.Commandes.FindAsync(idCommande);

            string url = $"/annonce/{idannonce}";
            string titre = "Paiemement";
            string message = $"Votre offre pour l'annonce #{annonce.Libelle} à {montant}€ à été {(estAccepte? "accepté" : "refusé")}";
            string type = $"offre{(estAccepte? "accepte" : "refuse")}";
            await NotifCreationAutoAsync(new List<int>(){idCompte}, url, titre, message, idannonce, type, valeurOffre: montant);
        }
    }
}