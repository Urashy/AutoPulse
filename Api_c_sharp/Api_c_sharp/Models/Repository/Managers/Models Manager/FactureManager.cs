using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FactureManager : WritableManager<Facture>, ReadableRepository<Facture>
    {
        public FactureManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<IEnumerable<Facture>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<Facture?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}
