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
            var targetDay = DateTime.UtcNow.AddDays(-7).Date;

            var tousPaiements = await dbSet
                .Where(pai => pai.IdMiseEnAvant != null)
                .OrderByDescending(pai => pai.DatePaiement)
                .ToListAsync();

            var derniersPaiementsParAnnonce = tousPaiements
                .GroupBy(pai => pai.IdAnnonce)
                .Select(group => group.First())
                .Where(pai => pai.DatePaiement.Date == targetDay)
                .ToList();

            if (!derniersPaiementsParAnnonce.Any())
                return paiementCrees;

            var annonceIds = derniersPaiementsParAnnonce.Select(p => p.IdAnnonce).ToList();
            var annonces = await context.Annonces
                .Where(a => annonceIds.Contains(a.IdAnnonce))
                .ToDictionaryAsync(a => a.IdAnnonce);

            var carteBancaireIds = derniersPaiementsParAnnonce.Select(p => p.IdCarteBancaire).ToList();
            var cartesBancaires = await context.CarteBancaires
                .Where(c => carteBancaireIds.Contains(c.IdCarteBancaire))
                .ToDictionaryAsync(c => c.IdCarteBancaire);

            foreach (var paiement in derniersPaiementsParAnnonce)
            {
                if (!annonces.TryGetValue(paiement.IdAnnonce, out var annonce))
                    continue;
                if (!cartesBancaires.TryGetValue((int)paiement.IdCarteBancaire, out var carteBancaire))
                    continue;

                if (annonce.ProchaineMiseEnAvant == null)
                {
                    if (annonce.IdMiseEnAvant == 1)
                        continue;

                    var newPaiement = new Paiement
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
                else if (annonce.ProchaineMiseEnAvant > 1)
                {
                    var newPaiement = new Paiement
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
            }

            await context.SaveChangesAsync();

            return paiementCrees;
        }
    }
}

