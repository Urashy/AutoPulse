using Api_c_sharp.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FavoriManager : WriteableReadableManager<Favori>
    {
        public FavoriManager(AutoPulseBdContext context) : base(context)
        {
        }
        public async Task<Favori?> GetByIdAsync(int idCompte, int idAnnonce)
        {
            return await dbSet
                .Where(f => f.IdCompte == idCompte && f.IdAnnonce == idAnnonce)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsync(int idCompte, int idAnnonce)
        {
            return await dbSet
                .AnyAsync(f => f.IdCompte == idCompte && f.IdAnnonce == idAnnonce);
        }

        public async Task<IEnumerable<Favori>> GetByCompteIdAsync(int idCompte)
        {
            return await dbSet
                .Where(f => f.IdCompte == idCompte)
                .Include(f => f.AnnonceFavoriNav)
                .ToListAsync();
        }

    }
}