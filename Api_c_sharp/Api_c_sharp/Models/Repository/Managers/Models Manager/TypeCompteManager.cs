using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class TypeCompteManager : ReadableManager<TypeCompte>, ITypeCompteRepository
    {
        public TypeCompteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<TypeCompte> GetTypeCompteByCompteId(int compteID)
        {
            return await dbSet
                .Include(tc => tc.Comptes)
                .FirstOrDefaultAsync(tc => tc.Comptes.Any(c => c.IdCompte == compteID));
        }

        public virtual async Task<IEnumerable<TypeCompte>> GetTypeComptesPourChercher()
        {
            return await dbSet.Where(tc => tc.Cherchable == true).ToListAsync();
        }

    }
}
