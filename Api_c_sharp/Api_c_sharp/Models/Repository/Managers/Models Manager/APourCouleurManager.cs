using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class APourCouleurManager : WriteableReadableManager<APourCouleur> , IAPourCouleurRepository
    {
        public APourCouleurManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<APourCouleur> GetAPourCouleursByIDS(int voitureId, int couleurId)
        {
            return await dbSet.FirstOrDefaultAsync(apc => apc.IdVoiture == voitureId && apc.IdCouleur == couleurId);         
        }
    }
}
