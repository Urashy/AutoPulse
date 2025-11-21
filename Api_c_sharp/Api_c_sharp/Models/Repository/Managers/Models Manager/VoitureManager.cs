using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class VoitureManager : WriteableReadableManager<Voiture>
    {
        public VoitureManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Voiture>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Voiture?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}
