using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : SearchableManager<Annonce>, IRepository<Annonce>, WritableRepository<Annonce>
    {
        public AnnonceManager(context context) : base(context)
        { 
            
        }

        public Task<Annonce> AddAsync(Annonce entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Annonce entity)
        {
            throw new NotImplementedException();
        }

        public Task<Annonce?> GetByNameAsync(string name)
        {
            return await dbSet
               .Include(m => m.Annonces)
               .FirstOrDefaultAsync(m => m.Nom == name);
        }

        public Task UpdateAsync(Annonce entity)
        {
            throw new NotImplementedException();
        }
    }
}
