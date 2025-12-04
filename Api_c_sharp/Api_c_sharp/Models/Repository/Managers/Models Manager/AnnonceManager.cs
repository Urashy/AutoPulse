using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using AutoPulse.Shared.DTO;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : BaseManager<Annonce,string>, IAnnonceRepository
    {
        private IQueryable<Annonce> ApplyIncludes()
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
            return await ApplyIncludes().Where(a => a.IdEtatAnnonce == 1).OrderByDescending(a => a.IdMiseEnAvant).ToListAsync();
        }

        public override async Task<Annonce?> GetByNameAsync(string name)
        {
            return await ApplyIncludes().Where(a => a.IdEtatAnnonce == 1).FirstOrDefaultAsync(a => a.Libelle == name);
        }

        public async Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId, int pageNumber, int pageSize)
        {
            int skip = Math.Max(0, (pageNumber - 1) * pageSize);
            int take = Math.Max(1, pageSize);
            return await ApplyIncludes()
                .Where(a => a.IdMiseEnAvant == miseAvantId && a.IdEtatAnnonce == 1)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        // Dans AnnonceManager.cs - Méthode GetFilteredAnnonces

        public async Task<IEnumerable<Annonce>> GetFilteredAnnonces(ParametreRecherche param, int pageNumber, int pageSize, int orderprix)
        {
            var query = ApplyIncludes();

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

            // Filtre par type de vendeur
            if (param.IdTypeVendeur > 0)
                query = query.Where(a => a.CompteAnnonceNav.IdTypeCompte == param.IdTypeVendeur);

            // Filtre par nom
            if (!string.IsNullOrEmpty(param.Nom))
            {
                query = query.Where(a => Fuzz.PartialRatio(a.Libelle.ToLower(), param.Nom.ToLower()) > 70);
            }

            if (param.KmMin > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage >= param.KmMin);
            if (param.KmMax > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage <= param.KmMax);

            // Tri avec priorité : Mise en avant > Prix > Date
            IOrderedQueryable<Annonce> orderedQuery = query.OrderByDescending(a => a.IdMiseEnAvant);

            // Tri par prix selon le paramètre orderprix
            if (orderprix == 1) // Croissant
                orderedQuery = orderedQuery.ThenBy(a => a.Prix);
            else if (orderprix == 2) // Décroissant
                orderedQuery = orderedQuery.ThenByDescending(a => a.Prix);

            orderedQuery = orderedQuery.ThenByDescending(a => a.DatePublication);

            int skip = Math.Max(0, (pageNumber - 1) * pageSize);
            int take = Math.Max(1, pageSize); // Assure qu'on prend au moins 1 élément

            var result = await orderedQuery
                .Skip(skip)
                .Take(take)
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<Annonce>> GetAnnoncesByCompteFavoris(int compteId)
        {
            return await dbSet.Where(a => a.Favoris.Any(f => f.IdCompte == compteId)).ToListAsync();
        }
        public override async Task<Annonce?> GetByIdAsync(int id)
        {
            return await ApplyIncludes()
                .FirstOrDefaultAsync(a => a.IdAnnonce == id);
        }

        public async Task<IEnumerable<Annonce>> GetAnnoncesByCompteID(int compteId)
        {
            return await ApplyIncludes()
                .Where(a => a.IdCompte == compteId)
                .ToListAsync();
        }

        public override async Task DeleteAsync(Annonce entity)
        {
            Commande commandes = await context.Commandes
                .FirstOrDefaultAsync(c => c.IdAnnonce == entity.IdAnnonce);

            if (commandes != null)
                throw new InvalidOperationException("Impossible de supprimer une annonce deja commander.");
            else
                await base.DeleteAsync(entity);
        }
    }
}