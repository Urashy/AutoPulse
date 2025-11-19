using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api_c_sharp.Models.Repository.Managers
{

    public abstract class BaseManager<TEntity, TKey> : IDataRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly AutoPulseBdContext context;
        protected readonly DbSet<TEntity> dbSet;

        public BaseManager(AutoPulseBdContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(TEntity entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public abstract Task<TEntity?> GetByNameAsync(TKey name);

        public async Task UpdateAsync(TEntity entityToUpdate, TEntity entity)
        {
            context.Entry(entityToUpdate).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
        }
    }
    
}
