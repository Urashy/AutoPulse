using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class PaysManager : ReadableManager<Pays>
    {
        public PaysManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Pays>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.Libelle).ToListAsync();
        }
    }
}
