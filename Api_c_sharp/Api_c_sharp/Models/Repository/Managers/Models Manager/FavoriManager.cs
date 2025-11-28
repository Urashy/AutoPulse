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

        // Méthode pour vérifier si un favori existe
        public async Task<bool> ExistsAsync(int idCompte, int idAnnonce)
        {
            return await dbSet
                .AnyAsync(f => f.IdCompte == idCompte && f.IdAnnonce == idAnnonce);
        }

        // Méthode pour récupérer tous les favoris d'un compte
        public async Task<IEnumerable<Favori>> GetByCompteIdAsync(int idCompte)
        {
            return await dbSet
                .Where(f => f.IdCompte == idCompte)
                .Include(f => f.AnnonceFavoriNav)
                .ToListAsync();
        }
    }
}