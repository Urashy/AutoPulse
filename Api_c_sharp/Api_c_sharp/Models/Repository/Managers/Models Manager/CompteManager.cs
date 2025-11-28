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

        public override async Task<Compte> GetByIdAsync(int id)
        {
            return await dbSet.Include(c => c.Images).FirstOrDefaultAsync(c => c.IdCompte == id);
        }

        public override async Task<Compte?> GetByNameAsync(string mail)
        {
            return await dbSet.Where(c => c.Email == mail).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Compte>> GetCompteByIdAnnonceFavori(int annonceId)
        {
            return await dbSet.Where(c => c.Favoris.Any(a => a.IdAnnonce == annonceId)).ToListAsync();
        }

        public async Task<IEnumerable<Compte>> GetComptesByTypes(int type)
        {
            return await dbSet.Where(c => c.IdTypeCompte == type).ToListAsync();
        }

        public async Task<Compte> VerifMotDePasse(string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.MotDePasse == hash);
        }

        public async Task<Compte> AuthenticateCompte(string email, string hash)
        {
            return await dbSet.SingleOrDefaultAsync(x => x.Email.ToUpper() == email.ToUpper() && 
                                                  x.MotDePasse == hash);
        }
    }
}
