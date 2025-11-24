using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CouleurManager : ReadableManager<Couleur>, ICouleurRepository
    {
        public CouleurManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Couleur>> GetCouleursByVoitureId(int voitureId)
        {
            return await dbSet.Where(c => c.APourCouleurs.Any(ac => ac.IdVoiture == voitureId)).ToListAsync();
        }
    }
}
