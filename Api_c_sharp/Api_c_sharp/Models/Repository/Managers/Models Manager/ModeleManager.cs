using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class ModeleManager : ReadableManager<Modele>, IModeleRepository
    {

        public ModeleManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Modele>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.LibelleModele).ToListAsync();
        }

        public virtual async Task<IEnumerable<Modele>> GetModelesByMarqueIdAsync(int marqueId)
        {
            return await dbSet.Where(m => m.IdMarque == marqueId).OrderBy(s => s.LibelleModele).ToListAsync();
        }
    }
}
