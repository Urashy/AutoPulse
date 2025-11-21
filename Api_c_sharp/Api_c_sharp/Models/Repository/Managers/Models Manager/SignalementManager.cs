using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class SignalementManager : WriteableReadableManager<Signalement>, ISignalementRepository
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
