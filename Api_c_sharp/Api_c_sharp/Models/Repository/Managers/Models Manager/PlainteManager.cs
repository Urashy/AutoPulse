using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class PlainteManager : WriteableReadableManager<Plainte>, IPlainteRepository
    {
        public PlainteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Plainte>> GetAllAsync()
        {
            return await dbSet.OrderBy(p => p.DateCreation).ToListAsync();
        }

        public async Task<IEnumerable<Plainte>> GetPlainteByCompteID(int idCompte)
        {
            return await dbSet.Where(p => p.IdCompte == idCompte)
                .OrderBy(p => p.DateCreation)
                .ToListAsync();
        }
    }
}
