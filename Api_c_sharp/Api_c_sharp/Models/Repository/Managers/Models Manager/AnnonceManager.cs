using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class AnnonceManager : BaseManager<Annonce,string>, IAnnonceRepository
    {
        private IQueryable<Annonce> Api_c_sharplyIncludes()
        {
            return context.Set<Annonce>()
                .Include(a => a.MiseEnAvantAnnonceNav)
                .Include(a => a.CompteAnnonceNav)
                    .ThenInclude(c => c.TypeCompteCompteNav)
                .Include(a => a.EtatAnnonceNavigation)
                .Include(a => a.AdresseAnnonceNav)
                    .ThenInclude(adr => adr.PaysAdresseNav)

                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.MarqueVoitureNavigation)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.ModeleVoitureNavigation)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.CarburantVoitureNavigation)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.BoiteVoitureNavigation) 
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.MotriciteVoitureNavigation) 
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.CategorieVoitureNavigation)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.APourCouleurs)
                        .ThenInclude(ac => ac.APourCouleurCouleurNav)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.Images)
                .Include(a => a.VoitureAnnonceNav)
                    .ThenInclude(v => v.ModeleBlenderNavigation);
        }

        public AnnonceManager(AutoPulseBdContext context) : base(context)
        { 
            
        }

        public override async Task<IEnumerable<Annonce>> GetAllAsync()
        {
            return await Api_c_sharplyIncludes().Where(a => a.IdEtatAnnonce == 1).OrderByDescending(a => a.IdMiseEnAvant).ToListAsync();
        }

        public override async Task<Annonce?> GetByNameAsync(string name)
        {
            return await Api_c_sharplyIncludes().Where(a => a.IdEtatAnnonce == 1).FirstOrDefaultAsync(a => a.Libelle == name);
        }

        public virtual async Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId, int pageNumber, int pageSize)
        {
            int skip = Math.Max(0, (pageNumber - 1) * pageSize);
            int take = Math.Max(1, pageSize);
            return await Api_c_sharplyIncludes()
                .Where(a => a.IdMiseEnAvant == miseAvantId && a.IdEtatAnnonce == 1 && a.IdEtatAnnonce == 1)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        // Dans AnnonceManager.cs - Méthode GetFilteredAnnonces

        public virtual async Task<IEnumerable<Annonce>> GetFilteredAnnonces(ParametreRecherche param)
        {
            var query = Api_c_sharplyIncludes();

            // Filtre par département
            if (!string.IsNullOrEmpty(param.Departement))
                query = query.Where(a => a.AdresseAnnonceNav.CodePostal == param.Departement);

            // Filtre par carburant
            if (param.IdCarburant > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdCarburant == param.IdCarburant);

            // Filtre par marque
            if (param.IdMarque > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdMarque == param.IdMarque);

            // Filtre par modèle
            if (param.IdModele > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdModele == param.IdModele);

            // Filtre par prix
            if (param.PrixMin > 0)
                query = query.Where(a => a.Prix >= param.PrixMin);
            if (param.PrixMax > 0)
                query = query.Where(a => a.Prix <= param.PrixMax);

            // Filtre par type de voiture
            if (param.IdTypeVoiture > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdCategorie == param.IdTypeVoiture);

            if (param.IdTypeVendeur > 0)
                query = query.Where(a => a.CompteAnnonceNav.IdTypeCompte == param.IdTypeVendeur);

            if (!string.IsNullOrEmpty(param.Nom))
            {
                query = query.Where(a => Fuzz.PartialRatio(a.Libelle.ToLower(), param.Nom.ToLower()) > 70);
            }

            if (param.IdBoitedevitesse > 0)
            {
                query = query.Where(a => a.VoitureAnnonceNav.IdBoiteDeVitesse == param.IdBoitedevitesse);
            }

            if (param.KmMin > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage >= param.KmMin);
            if (param.KmMax > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage <= param.KmMax);

            query = query.Where(a => a.IdEtatAnnonce == 1);

            IOrderedQueryable<Annonce> orderedQuery = query.OrderByDescending(a => a.IdMiseEnAvant);

            query = param.Order switch
            {
                1 => query.OrderBy(a => a.Prix),              // Prix croissant
                2 => query.OrderByDescending(a => a.Prix),    // Prix décroissant
                3 => query.OrderBy(a => a.DatePublication),   // Date croissant
                4 => query.OrderByDescending(a => a.DatePublication), // Date décroissant
                _ => query.OrderByDescending(a => a.DatePublication)  // Par défaut
            };

            int skip = Math.Max(0, (param.PageNumber - 1) * param.PageSize);
            int take = Math.Max(1, param.PageSize);

            var result = await orderedQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return result;
        }

        public virtual async Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId)
        {
            return await Api_c_sharplyIncludes().Where(a => a.Favoris.Any(f => f.IdCompte == compteId) && a.IdEtatAnnonce == 1).ToListAsync();
        }
        public override async Task<Annonce?> GetByIdAsync(int id)
        {
            return await Api_c_sharplyIncludes()
                .FirstOrDefaultAsync(a => a.IdAnnonce == id);
        }

        public virtual async Task<IEnumerable<Annonce>> GetAnnoncesByCompteID(int compteId)
        {
            return await Api_c_sharplyIncludes()
                .Where(a => a.IdCompte == compteId)
                .ToListAsync();
        }

        public override async Task<bool> DeleteAsync(Annonce entity)
        {
            Commande commandes = await context.Commandes.FirstOrDefaultAsync(c => c.IdAnnonce == entity.IdAnnonce);

            if (commandes != null)
            {
                entity.IdEtatAnnonce = 6;
                return false;
            }
            else
                await base.DeleteAsync(entity);
            return true;
        }

        public async Task<bool> EstMasque(int annonceId)
        {
            Annonce annonce = dbSet.Find(annonceId);
            if (annonce.IdEtatAnnonce == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}