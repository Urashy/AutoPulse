using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class AvisManager : WritableManager<Avis>, ReadableRepository<Avis>, IAvisRepository
    {
        public AvisManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Avis>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<IEnumerable<Avis>> GetAvisByCompteId(int compteId)
        {
            return await dbSet.Where(c => c.IdJugeur == compteId || c.IdJugee == compteId ).ToListAsync();
        }

        public async Task<Avis?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}