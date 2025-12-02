using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class JournalManager : WriteableReadableManager<Journal>, IJournalRepository
    {
        public JournalManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Journal>> GetJournalByType(int typeID)
        {
            return await dbSet.Where(journal => journal.IdTypeJournal == typeID).OrderBy(j => j.DateJournal).ToListAsync();
        }
    }
}
