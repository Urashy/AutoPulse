using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CompteManager : BaseManager<Compte,string> , ICompteRepository
    {
        public CompteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<Compte?> GetByNameAsync(string name)
        {
            return await dbSet.Where(c => c.Pseudo == name).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId)
        {
            return await dbSet.Where(c => c.Favoris.Any(a => a.IdAnnonce == annonceId)).ToListAsync();
        }

        public async Task<IEnumerable<Compte>> GetComptesByTypes(int type)
        {
            return await dbSet.Where(c => c.IdTypeCompte == type).ToListAsync();
        }
    }
}
