using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CompteManager : BaseManager<Compte,string> , ICompteRepository
    {
        public CompteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<Compte> GetByIdAsync(int id)
        {
            return await dbSet.Include(c => c.Images).Include(c => c.TypeCompteCompteNav).FirstOrDefaultAsync(c => c.IdCompte == id);
        }

        public override async Task<Compte?> GetByNameAsync(string mail)
        {
            return await dbSet.Where(c => c.Email == mail).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId)
        {
            return await dbSet.Where(c => c.Favoris.Any(a => a.IdAnnonce == annonceId)).ToListAsync();
        }

        public async Task<IEnumerable<Compte>> GetComptesByTypes(int type)
        {
            return await dbSet.Where(c => c.IdTypeCompte == type).ToListAsync();
        }

        public async Task<Compte> VerifMotDePasse(string email, string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.Email == email && x.MotDePasse == hash);
        }

        public async Task<Compte> AuthenticateCompte(string email, string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.Email.ToUpper() == email.ToUpper() && 
                                                  x.MotDePasse == hash);
        }

        public async Task UpdateAnonymise(int idcompte)
        {
            Compte compte = await dbSet
                .Include(c => c.CommandeAcheteur)
                .Include(c => c.Annonces)
                .Include(c => c.SignalementsFaits)
                .Include(c => c.SignalementsRecus)
                .Include(c => c.CommandeVendeur)
                .Include(c => c.AvisJugees)
                .Include(c => c.AvisJugeur)
                .FirstOrDefaultAsync(c => c.IdCompte == idcompte);

            if (compte == null)
                return;

            // ✅ Vérifier si les collections sont null avant d'appeler .Any()
            bool aDesActivites = (compte.CommandeAcheteur?.Any() ?? false) ||
                                 (compte.Annonces?.Any() ?? false) ||
                                 (compte.SignalementsFaits?.Any() ?? false) ||
                                 (compte.SignalementsRecus?.Any() ?? false) ||
                                 (compte.CommandeVendeur?.Any() ?? false) ||
                                 (compte.AvisJugees?.Any() ?? false) ||
                                 (compte.AvisJugeur?.Any() ?? false);

            if (!aDesActivites)
            {
                // Supprimer les données liées
                List<Adresse> adressesASupprimer = await context.Adresses
                    .Where(a => a.IdCompte == compte.IdCompte)
                    .ToListAsync();
                if (adressesASupprimer.Any())
                    context.Adresses.RemoveRange(adressesASupprimer);

                List<Favori> favorisASupprimer = await context.Favoris
                    .Where(f => f.IdCompte == compte.IdCompte)
                    .ToListAsync();
                if (favorisASupprimer.Any())
                    context.Favoris.RemoveRange(favorisASupprimer);

                List<Journal> logsASupprimer = await context.Journaux
                    .Where(l => l.IdCompte == compte.IdCompte)
                    .ToListAsync();
                if (logsASupprimer.Any())
                    context.Journaux.RemoveRange(logsASupprimer);

                // Supprimer le compte
                dbSet.Remove(compte);
                await context.SaveChangesAsync();
            }
            else
            {
                // Anonymiser le compte
                compte.MotDePasse = "XXXXXXXXX";
                compte.Nom = "ANONYME";
                compte.Prenom = "Utilisateur";
                compte.Email = $"anonyme_{compte.IdCompte}_{Guid.NewGuid().ToString().Substring(0, 8)}@deleted.com";
                compte.Pseudo = $"anonyme_{compte.IdCompte}_{Guid.NewGuid().ToString().Substring(0, 3)}";
                compte.DateNaissance = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                compte.IdTypeCompte = 4;
                compte.Biographie = null;

                // ✅ Pas besoin de SetValues, on modifie directement l'entité trackée
                await context.SaveChangesAsync();
            }
        }
        public async Task<int?> GetTypeCompteByCompteId(int compteId)
        {
            var compte = await dbSet
                .Where(c => c.IdCompte == compteId)
                .Select(c => c.IdTypeCompte)
                .FirstOrDefaultAsync();

            return compte == 0 ? null : compte;
        }
    }
}
