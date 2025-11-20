using Api_c_sharp.Models.Repository.Interfaces;
using System.Data.Entity;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class JournalManager : WritableManager<Journal>, ReadableRepository<Journal>, IJournalRepository
    {
        public JournalManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Journal>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Journal?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Journal>> GetJournalByType(int typeID)
        {
            return await dbSet.Where(journal => journal.IdTypeJournal == typeID).ToListAsync();
        }
    }
}
