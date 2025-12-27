using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class OffreManager : WriteableReadableManager<Offre> , IOffreRepository
    {
        public OffreManager(AutoPulseBdContext context) : base(context)
        {
        }
        public virtual async Task<IEnumerable<Offre>> GetOffresByMessageIdAsync(int idMessage)
        {
            return await dbSet
                .Where(o => o.IdMessage == idMessage)
                .OrderByDescending(o => o.DateOffre)
                .ToListAsync();
        }
    }
}
