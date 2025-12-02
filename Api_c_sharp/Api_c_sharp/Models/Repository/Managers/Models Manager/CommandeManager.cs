using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class CommandeManager : WriteableReadableManager<Commande>, ICommandeRepository
    {
        public CommandeManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<Commande>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.Date).ToListAsync();
        }

        public async Task<IEnumerable<Commande>> GetCommandeByCompteId(int compteId)
        {
            return await dbSet.Where(commande => commande.IdAcheteur == compteId).ToListAsync();
        }

    }
}
