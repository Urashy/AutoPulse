using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class TypeJournalManager : ReadableManager<TypeJournal>
    {
        public TypeJournalManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<TypeJournal>> GetAllAsync()
        {
            return await dbSet
                .OrderBy(m => m.LibelleTypeJournaux)
                .ToListAsync();
        }
    }
}
