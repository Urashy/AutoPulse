using Api_c_sharp.Models.Repository.Interfaces;
using System.Data.Entity;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AdresseManager : WritableManager<Adresse>, ReadableRepository<Adresse>
    {
        public AdresseManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Adresse>> GetAllAsync()
        {
            {
                return await dbSet.ToListAsync();
            }
            ;
        }

        public async Task<Adresse?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}
