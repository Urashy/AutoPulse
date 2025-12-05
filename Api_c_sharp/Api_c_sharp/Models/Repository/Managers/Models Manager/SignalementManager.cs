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
            return await dbSet.OrderBy(s => s.DateCreationSignalement).ToListAsync();
        }

        public virtual async Task<IEnumerable<Signalement>> GetSignalementsByEtat(int etatId)
        {
            return await dbSet.Where(s => s.IdEtatSignalement == etatId).ToListAsync();
        }
    }
}
