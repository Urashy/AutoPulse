using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MoyenPaiementManager : ReadableManager<MoyenPaiement>
    {
        public MoyenPaiementManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<MoyenPaiement>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.TypePaiement).ToListAsync();
        }
    }
}
