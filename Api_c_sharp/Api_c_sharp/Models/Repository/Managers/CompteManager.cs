using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CompteManager : SearchableManager<Compte>, WritableRepository<Compte>
    {
        public CompteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public Task<Compte> AddAsync(Compte entity)
        {

        }

        public Task DeleteAsync(Compte entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Compte entity)
        {
            throw new NotImplementedException();
        }
    }
}
