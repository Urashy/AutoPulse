using Api_c_sharp.Models.Repository.Interfaces;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

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
            return await ApplyIncludes().OrderByDescending(a => a.IdMiseEnAvant).ToListAsync();
        }

        public override async Task<Annonce?> GetByNameAsync(string name)
        {
            return await ApplyIncludes().FirstOrDefaultAsync(a => a.Libelle == name);
        }

        public async Task<IEnumerable<Annonce>> GetAnnoncesByMiseEnAvant(int miseAvantId)
        {
            return await ApplyIncludes()
                .Where(a => a.IdMiseEnAvant == miseAvantId)
                .ToListAsync();
        }


        public async Task<IEnumerable<Annonce>> GetFilteredAnnonces(int id, int idcarburant, int idmarque, int idmodele, int prixmin, int prixmax, int idtypevoiture, int idtypevendeur, string nom, int kmmin, int kmmax, string departement, int pageNumber, int pageSize)
        {
            var query = ApplyIncludes();

            // Filtre par département
            if (!string.IsNullOrEmpty(departement))
                query = query.Where(a => a.AdresseAnnonceNav.CodePostal == departement);

            // Filtre par carburant
            if (idcarburant > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdCarburant == idcarburant);

            // Filtre par marque
            if (idmarque > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdMarque == idmarque);

            // Filtre par modèle
            if (idmodele > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdModele == idmodele);

            // Filtre par prix
            if (prixmin > 0)
                query = query.Where(a => a.Prix >= prixmin);
            if (prixmax > 0)
                query = query.Where(a => a.Prix <= prixmax);

            // Filtre par type de voiture
            if (idtypevoiture > 0)
                query = query.Where(a => a.VoitureAnnonceNav.IdCategorie == idtypevoiture);

            // Filtre par type de vendeur
            if (idtypevendeur > 0)
                query = query.Where(a => a.CompteAnnonceNav.IdTypeCompte == idtypevendeur);

            // Filtre par nom
            if (!string.IsNullOrEmpty(nom))
            {
                query = query.Where(a => Fuzz.PartialRatio(a.Libelle.ToLower(), nom.ToLower()) > 70);
            }

            if (kmmin > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage >= kmmin);
            if (kmmax > 0)
                query = query.Where(a => a.VoitureAnnonceNav.Kilometrage <= kmmax);

            // Ajout du tri par défaut avant pagination
            query = query
                .OrderByDescending(a => a.IdMiseEnAvant)
                .ThenByDescending(a => a.DatePublication);

            // Logique de pagination
            if (pageNumber > 0 && pageSize > 0)
            {
                query = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }

            return await query.ToListAsync();
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
    }
}