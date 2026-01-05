using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class BloqueManager : WriteableReadableManager<Bloque>, IBloqueRepository
    {
        public BloqueManager(AutoPulseBdContext context) : base(context)
        {
            
        }

        public virtual async Task<bool> ExistsAsync(int idComptebloque, int idcomptebloquant)
        {
            return await dbSet.AnyAsync(b => b.IdBloque == idComptebloque && b.IdBloquant == idcomptebloquant);
        }

        public virtual async Task<Bloque?> GetBloqueByIdsAsync(int idBloque, int idBloquant)
        {
            return await dbSet.FindAsync(idBloque, idBloquant);
        }
    }
}
