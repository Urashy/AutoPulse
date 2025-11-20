using Api_c_sharp.Models.Repository.Interfaces;
using System.Data.Entity;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class SignalementManager : WritableManager<Signalement>, ReadableRepository<Signalement>, ISignalementRepository
    {
        public SignalementManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Signalement>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Signalement?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Signalement>> GetSignalementsByEtat(int etatId)
        {
            return await dbSet.Where(s => s.IdEtatSignalement == etatId).ToListAsync();
        }
    }
}
