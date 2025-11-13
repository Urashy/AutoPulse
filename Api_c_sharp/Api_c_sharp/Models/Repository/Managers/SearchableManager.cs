using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public abstract class SearchableManager<TEntity> : BaseManager<TEntity>, SearchableRepository<TEntity>
    where TEntity : class, SearchableEntity
    {
        protected SearchableManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<TEntity?> GetByNameAsync(string name)
        {
            return await dbSet.FirstOrDefaultAsync(e => e.Nom == name);
        }
    }
}
