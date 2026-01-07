using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class PaiementManager : WriteableReadableManager<Paiement>, IPaiementRepository
    {
        public PaiementManager(AutoPulseBdContext context) : base(context)
        {
        }

        public Task<IEnumerable<Paiement>> GetPaimentByAnnonce(int idannonce)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> VerifPaiementAutoMiseEnAvant()
        {
            try
            {
                List<Paiement> paiements = await dbSet
                    .Where(pai => pai.IdMiseEnAvant != 0 && pai.DatePaiement <= DateTime.Now.AddDays(-7))
                    .ToListAsync();

                if (!paiements.Any())
                    return true;

                Annonce annonce = new Annonce();

                foreach (var paiement in paiements)
                {
                    annonce = await context.Annonces.FirstOrDefaultAsync(ann => ann.IdAnnonce == paiement.IdAnnonce);
                    if (annonce.IdMiseEnAvant == annonce.ProchaineMiseEnAvant || annonce.ProchaineMiseEnAvant == 1)
                    {
                        if (annonce.ProchaineMiseEnAvant == 1)
                        {
                            annonce.IdMiseEnAvant = 1;
                            return true;
                        }
                        Paiement newPaiement = new Paiement
                        {
                            IdAnnonce = paiement.IdAnnonce,
                            IdCarteBancaire = paiement.IdCarteBancaire,
                            IdCommande = paiement.IdCommande,
                            IdCompte = paiement.IdCompte,
                            IdMiseEnAvant = annonce.ProchaineMiseEnAvant ?? 1,
                            DatePaiement = DateTime.Now
                        };
                        await AddAsync(newPaiement);  
                    }
                    else
                    {
                        annonce.IdMiseEnAvant = annonce.ProchaineMiseEnAvant ?? 1;
                        context.Annonces.Update(annonce);
                        paiement.DatePaiement = DateTime.Now;
                        paiement.IdMiseEnAvant = annonce.IdMiseEnAvant;
                        await AddAsync(paiement);
                    }
                }
                await context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
