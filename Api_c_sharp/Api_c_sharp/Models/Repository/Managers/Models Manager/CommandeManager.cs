using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class CommandeManager : WriteableReadableManager<Commande>, ICommandeRepository
    {
        public CommandeManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Commande>> GetCommandeByCompteId(int compteId)
        {
            return await dbSet.Where(commande => commande.IdAcheteur == compteId).ToListAsync();
        }

    }
}
