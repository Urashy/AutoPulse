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
        
        public async Task<IEnumerable<Paiement>> VerifPaiementAutoMiseEnAvant()
        {                
            List<Paiement> paiementCrees = new List<Paiement>();
            try
            {
                var targetDay = DateTime.Now.AddDays(-7);
                List<Paiement> paiements = await dbSet
                    .Where(pai =>
                        pai.IdMiseEnAvant != null &&
                        pai.DatePaiement.Day == targetDay.Day &&
                        pai.DatePaiement.Month == targetDay.Month &&
                        pai.DatePaiement.Year == targetDay.Year
                    )
                    .GroupBy(pai => pai.IdAnnonce)
                    .Select(group => group
                        .OrderByDescending(pai => pai.DatePaiement)
                        .FirstOrDefault())
                    .ToListAsync();

                if (!paiements.Any())
                    return paiementCrees;

                Annonce annonce = new Annonce();

                foreach (var paiement in paiements)
                {
                    annonce = await context.Annonces.FirstOrDefaultAsync(ann => ann.IdAnnonce == paiement.IdAnnonce);
                    
                    if (annonce == null)
                        return paiementCrees;
                    
                    if (annonce.ProchaineMiseEnAvant == null)
                    {
                        if (annonce.IdMiseEnAvant == 1)
                            return paiementCrees;
                        
                        Paiement newPaiement = new Paiement
                        {
                            IdAnnonce = paiement.IdAnnonce,
                            IdMiseEnAvant = paiement.IdMiseEnAvant,
                            DatePaiement = DateTime.UtcNow,
                            IdCompte = paiement.IdCompte,
                            IdCarteBancaire = paiement.IdCarteBancaire
                        };
                        paiementCrees.Add(newPaiement);
                        await context.Paiements.AddAsync(newPaiement);
                    }
                    else if (annonce.ProchaineMiseEnAvant >= 1)
                    {
                        Paiement newPaiement = new Paiement
                        {
                            IdAnnonce = paiement.IdAnnonce,
                            IdMiseEnAvant = annonce.ProchaineMiseEnAvant,
                            DatePaiement = DateTime.UtcNow,
                            IdCompte = paiement.IdCompte,
                            IdCarteBancaire = paiement.IdCarteBancaire
                        };
                        paiementCrees.Add(newPaiement);
                        annonce.IdMiseEnAvant = (int)annonce.ProchaineMiseEnAvant;
                        annonce.ProchaineMiseEnAvant = null;
                        await context.Paiements.AddAsync(newPaiement);
                        
                    }
                    else if (annonce.ProchaineMiseEnAvant == 1)
                    {
                        annonce.IdMiseEnAvant = (int)annonce.ProchaineMiseEnAvant;
                        annonce.ProchaineMiseEnAvant = null;
                    }
                    await context.SaveChangesAsync();
                }
                return paiementCrees;
            }
            catch
            {
                return paiementCrees;
            }
        }
    }
}
