using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : SearchableManager<Annonce>, WritableRepository<Annonce>
    {
        public AnnonceManager(AutoPulseBdContext context) : base(context)
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

        public Task UpdateAsync(Annonce entity)
        {
            throw new NotImplementedException();
        }
    }
}
