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
            return await dbSet.Include(a=> a.AcheteurCommande).Include(v=> v.VendeurCommande).Include(x=>x.CommandeEtatCommandeNav).Include(x=>x.CommandeMoyenPaiementNav).Include(ann=>ann.CommandeAnnonceNav).Include(c=>c.Offrecommande).OrderBy(s => s.Date).ToListAsync();
        }

        public virtual async Task<IEnumerable<Commande>> GetCommandesByCompteId(int compteId)
        {
            return await dbSet.Include(commande => commande.CommandeAnnonceNav)
                .ThenInclude(annonce => annonce.CompteAnnonceNav)
                .Include(commande => commande.CommandeMoyenPaiementNav)
                .Include(na => na.AcheteurCommande)
                .Where(commande => commande.IdAcheteur == compteId).ToListAsync();
        }
        public override async Task<Commande?> GetByIdAsync(int id)
        {
            return await dbSet.Include(a => a.AcheteurCommande)
                .Include(v => v.VendeurCommande)
                .Include(x => x.CommandeEtatCommandeNav)
                .Include(x => x.CommandeMoyenPaiementNav)
                .Include(ann => ann.CommandeAnnonceNav)
                .Include(c => c.Offrecommande)
                .FirstOrDefaultAsync(c => c.IdCommande == id);
        }

    }
}
