using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class BloqueManager : WriteableReadableManager<Bloque>
    {
        public BloqueManager(AutoPulseBdContext context) : base(context)
        {
            
        }
        public virtual async Task<Bloque?> GetByIdsAsync(int idBloque, int idBloquant)
        {
            return await dbSet.FindAsync(idBloque, idBloquant);
        }
    }
}
