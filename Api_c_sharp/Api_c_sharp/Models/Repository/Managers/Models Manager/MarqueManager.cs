using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MarqueManager : ReadableManager<Marque>
    {
        public MarqueManager(AutoPulseBdContext context) : base(context)
        {
        }
        
        public override async Task<IEnumerable<Marque>> GetAllAsync()
        {
            return await dbSet.OrderBy(m => m.LibelleMarque).ToListAsync();
        }
    }
}
