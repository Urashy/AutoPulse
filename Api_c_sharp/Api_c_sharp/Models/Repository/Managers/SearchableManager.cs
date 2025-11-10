using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.Models.Repository.Managers
{
    public abstract class SearchableManager<TEntity> : ManagerGenerique<TEntity>, SearchableRepository<TEntity>
    where TEntity : class, SearchableEntity
    {
        protected SearchableManager(ProduitsbdContext context) : base(context)
        {
        }

        public virtual async Task<TEntity?> GetByNameAsync(string name)
        {
            return await dbSet.FirstOrDefaultAsync(e => e.Nom == name);
        }
    }
}
