using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class AvisManager : WriteableReadableManager<Avis>, IAvisRepository
    {
        public AvisManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<IEnumerable<Avis>> GetAvisByCompteId(int compteId)
        {
            return await dbSet.Include(a => a.CompteJugeeNav).Include(a => a.CompteJugeurNav).Where(c => c.IdJugeur == compteId || c.IdJugee == compteId ).OrderBy(a => a.DateAvis).ToListAsync();
        }
        public virtual async Task<bool> ExisteDejaAsync(int idCommande, int idJugeur)
        {
            return await dbSet.AnyAsync(a => a.IdCommande == idCommande && a.IdJugeur == idJugeur);
        }
    }
}