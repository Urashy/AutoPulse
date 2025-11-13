using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api_c_sharp.Models.Repository.Managers
{

    public abstract class BaseManager<TEntity> : IdataRepository<TEntity>
    {
        protected readonly AutoPulseBdContext context;
        protected readonly DbSet<TEntity> dbSet;

        public BaseManager(AutoPulseBdContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public Task<TEntity> AddAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public virtual async Task<TEntity?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public Task<TEntity?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TEntity entityToUpdate, TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
    
}
