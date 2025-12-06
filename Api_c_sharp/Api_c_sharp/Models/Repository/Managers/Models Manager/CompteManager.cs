using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using AutoPulse.Shared.DTO;
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

        public virtual async Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId)
        {
            return await dbSet.Where(c => c.Favoris.Any(a => a.IdAnnonce == annonceId)).ToListAsync();
        }

        public virtual async Task<IEnumerable<Compte>> GetComptesByTypes(int type)
        {
            return await dbSet.Where(c => c.IdTypeCompte == type).ToListAsync();
        }

        public virtual async Task<Compte> VerifMotDePasse(string email, string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.Email == email && x.MotDePasse == hash);
        }

        public virtual async Task<Compte> AuthenticateCompte(string email, string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.Email.ToUpper() == email.ToUpper() && x.MotDePasse == hash);
        }

        public virtual async Task UpdateAnonymise(int idcompte)
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
            Compte comptedebase = compte;

            if (compte == null)
                return;
            bool aDesActivites = (compte.CommandeAcheteur?.Any() ?? false) ||
                                 (compte.Annonces?.Any() ?? false) ||
                                 (compte.SignalementsFaits?.Any() ?? false) ||
                                 (compte.SignalementsRecus?.Any() ?? false) ||
                                 (compte.CommandeVendeur?.Any() ?? false) ||
                                 (compte.AvisJugees?.Any() ?? false) ||
                                 (compte.AvisJugeur?.Any() ?? false);

            if (!aDesActivites)
            {
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

                dbSet.Remove(compte);
                await context.SaveChangesAsync();
            }
            else
            {
                List<Annonce> annoncesaModifier = await context.Annonces
                    .Where(a => a.IdCompte == compte.IdCompte)
                    .ToListAsync();

                foreach (Annonce annonce in annoncesaModifier)
                {
                    annonce.IdEtatAnnonce = 5; 
                }


                compte.MotDePasse = "XXXXXXXXX"; 
                compte.Nom = "ANONYME";
                compte.Prenom = "Utilisateur";

                compte.Email = $"anonyme_{compte.IdCompte}_{Guid.NewGuid().ToString().Substring(0, 8)}@deleted.com";
                compte.Pseudo = $"anonyme_{compte.IdCompte}_{Guid.NewGuid().ToString().Substring(0, 3)}";
                compte.DateNaissance = DateTime.SpecifyKind(new DateTime(1900, 01, 01), DateTimeKind.Utc);
                compte.IdTypeCompte = 4;
                compte.Biographie = null;
                

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

        public async Task UpdateTypeCompte(Compte compteamodif,CompteModifTypeCompteDTO compteModifTypeCompteDTO, bool estpro)
        {
            if(estpro)
                compteamodif.IdTypeCompte = 1;
            else
                compteamodif.IdTypeCompte = 2;          

            compteamodif.NumeroSiret = compteModifTypeCompteDTO.NumeroSiret; 
            compteamodif.RaisonSociale = compteModifTypeCompteDTO.RaisonSociale;

            await context.SaveChangesAsync();
        }
    }
}
