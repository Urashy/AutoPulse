using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class SignalementManager : WriteableReadableManager<Signalement>, ISignalementRepository
    {
        public SignalementManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Signalement>> GetAllAsync()
        {
            return await dbSet.OrderByDescending(s => s.DateCreationSignalement)
                .Include(s => s.CompteSignalantNav)
                .Include(s => s.CompteSignaleNav)
                .Include(s => s.AnnonceSignaleNav)
                .Include(s => s.TypeSignalementSignalementNav)
                .Include(s => s.EtatSignalementNav).ToListAsync();
        }

        public virtual async Task<IEnumerable<Signalement>> GetSignalementsByEtatAndType(int etatId, int typeId, string recherche)
        {
            var query = dbSet.AsQueryable();

            if (etatId != 0)
            {
                query = query.Where(s => s.IdEtatSignalement == etatId);
            }


            if (typeId == 1)
            {
                query = query.Where(s => s.IdAnnonceSignale != null);
            }
            else if (typeId == 2)
            {
                query = query.Where(s => s.IdCompteSignale != null);
            }

            // 3. Filtre Recherche
            if (!string.IsNullOrWhiteSpace(recherche))
            {
                recherche = recherche.ToLower();
                query = query.Where(s => s.DescriptionSignalement.ToLower().Contains(recherche)
                    || s.CompteSignalantNav.Pseudo.ToLower().Contains(recherche)
                    || (s.CompteSignaleNav != null && s.CompteSignaleNav.Pseudo.ToLower().Contains(recherche))
                    || s.TypeSignalementSignalementNav.LibelleTypeSignalement.ToLower().Contains(recherche)
                    || (s.AnnonceSignaleNav != null && s.AnnonceSignaleNav.Libelle.ToLower().Contains(recherche)));
            }

            return await query
                .OrderByDescending(s => s.DateCreationSignalement)
                .Include(s => s.CompteSignalantNav)
                .Include(s => s.CompteSignaleNav)
                .Include(s => s.AnnonceSignaleNav)
                .Include(s => s.TypeSignalementSignalementNav)
                .Include(s => s.EtatSignalementNav)
                .ToListAsync();
        }

        public override async Task<Signalement?> GetByIdAsync(int id)
        {
            return await dbSet
                .Include(s => s.CompteSignalantNav)
                .Include(s => s.CompteSignaleNav)
                .Include(s => s.AnnonceSignaleNav)
                .Include(s => s.TypeSignalementSignalementNav)
                .Include(s => s.EtatSignalementNav)
                .FirstOrDefaultAsync(s => s.IdSignalement == id);
        }
    }
}