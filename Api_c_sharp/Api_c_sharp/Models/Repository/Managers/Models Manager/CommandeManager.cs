using Api_c_sharp.Models.Repository.Interfaces;
using System.Data.Entity;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class CommandeManager : WritableManager<Commande>, ReadableRepository<Commande>, ICommandeRepository
    {
        public CommandeManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Commande>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Commande?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Commande>> GetCommandeByCompteId(int compteId)
        {
            return await dbSet.Where(commande => commande.IdAcheteur == compteId).ToListAsync();
        }

    }
}
