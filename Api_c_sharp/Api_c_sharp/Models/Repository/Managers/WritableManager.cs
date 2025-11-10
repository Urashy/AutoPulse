using Api_c_sharp.Models.Repository.Interfaces;
using System.Data.Entity;

namespace Api_c_sharp.Models.Repository.Managers
{
    public abstract class WritableManager<TEntity> : ManagerGenerique<TEntity>, WritableRepository<TEntity>
    where TEntity : class
    {
        protected WritableManager(ProduitsbdContext context) : base(context)
        {
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(TEntity entity)
        {
            dbSet.Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
