using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AdresseManager : WriteableReadableManager<Adresse>, IAdresseRepository
    {
        public AdresseManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Adresse>> GetAdresseByCompteID(int compteId)
        {
           return await dbSet.Where(a => a.IdCompte == compteId).ToListAsync();
        }
    }
}
