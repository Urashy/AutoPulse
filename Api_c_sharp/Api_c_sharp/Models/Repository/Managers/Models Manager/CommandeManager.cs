using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using BlazorAutoPulse.Service;
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
            return await dbSet
                .Include(a => a.AcheteurCommande)
                .Include(v => v.VendeurCommande)
                .Include(x => x.EtatCommandeCommandeNav)
                .Include(x => x.CommandeMoyenPaiementNav)
                .Include(c => c.Offrecommande)
                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(v => v.MarqueVoitureNavigation)
                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(v => v.ModeleVoitureNavigation)
                 .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(v => v.CarburantVoitureNavigation)
                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.AdresseAnnonceNav)
                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.EtatAnnonceNavigation)
                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.CompteAnnonceNav)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public virtual async Task<IEnumerable<Commande>> GetCommandesByCompteId(int compteId)
        {
            return await dbSet
               .Include(c => c.CommandeMoyenPaiementNav)
               .Include(c => c.AcheteurCommande)
               .Include(c => c.CommandeAnnonceNav)
                   .ThenInclude(an => an.CompteAnnonceNav)
               .Include(c => c.CommandeAnnonceNav)
                   .ThenInclude(an => an.VoitureAnnonceNav)
                       .ThenInclude(v => v.MarqueVoitureNavigation)
               .Include(c => c.CommandeAnnonceNav)
                   .ThenInclude(an => an.AdresseAnnonceNav)
               .Where(commande => (commande.IdAcheteur == compteId || commande.IdVendeur == compteId))
               .ToListAsync();
        }

        public override async Task<Commande?> GetByIdAsync(int id)
        {
            return await dbSet
                .Include(a => a.AcheteurCommande)
                .Include(v => v.VendeurCommande)
                .Include(x => x.EtatCommandeCommandeNav)
                .Include(x => x.CommandeMoyenPaiementNav)
                .Include(c => c.Offrecommande)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.MarqueVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.ModeleVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.CarburantVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.AdresseAnnonceNav)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.EtatAnnonceNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.CompteAnnonceNav)

                .FirstOrDefaultAsync(c => c.IdCommande == id);
        }

        public virtual async Task<Commande> GetCommandeByConversation(int id)
        {
            return await dbSet
                .Include(a => a.AcheteurCommande)
                .Include(v => v.VendeurCommande)
                .Include(x => x.EtatCommandeCommandeNav)
                .Include(x => x.CommandeMoyenPaiementNav)
                .Include(c => c.Offrecommande)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.MarqueVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.ModeleVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.VoitureAnnonceNav)
                        .ThenInclude(voiture => voiture.CarburantVoitureNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.AdresseAnnonceNav)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.EtatAnnonceNavigation)

                .Include(ann => ann.CommandeAnnonceNav)
                    .ThenInclude(an => an.CompteAnnonceNav)
                    .FirstOrDefaultAsync(c => c.Offrecommande.OffreMessageNav.IdConversation == id);
        }

        public override async Task<Commande> AddAsync(Commande entity)
        {
            entity.Date = DateTime.UtcNow;

            return await base.AddAsync(entity);
        }

    }
}