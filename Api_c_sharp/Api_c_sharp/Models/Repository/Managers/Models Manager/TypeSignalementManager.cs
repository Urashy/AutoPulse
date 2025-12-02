using Api_c_sharp.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class TypeSignalementManager : ReadableManager<TypeSignalement>
    {
        public TypeSignalementManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<TypeSignalement>> GetAllAsync()
        {
            return await dbSet
                .OrderBy(m => m.LibelleTypeSignalement)
                .ToListAsync();
        }
    }
}
