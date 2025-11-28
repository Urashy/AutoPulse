using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CouleurManager : ReadableManager<Couleur>, ICouleurRepository
    {
        public CouleurManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Couleur>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.LibelleCouleur).ToListAsync();
        }

        public async Task<IEnumerable<Couleur>> GetCouleursByVoitureId(int voitureId)
        {
            return await dbSet.Where(c => c.APourCouleurs.Any(ac => ac.IdVoiture == voitureId)).ToListAsync();
        }
    }
}
