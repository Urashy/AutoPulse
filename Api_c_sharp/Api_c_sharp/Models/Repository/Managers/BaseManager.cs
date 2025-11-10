using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class BaseManager
    {
        public abstract class ManagerGenerique<TEntity> : IRepository<TEntity>
    where TEntity : class
        {
            protected readonly ProduitsbdContext context;
            protected readonly DbSet<TEntity> dbSet;

            public ManagerGenerique(ProduitsbdContext context)
            {
                this.context = context;
                this.dbSet = context.Set<TEntity>();
            }

            public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
            {
                return await dbSet.ToListAsync();
            }

            public virtual async Task<TEntity?> GetByIdAsync(int id)
            {
                return await dbSet.FindAsync(id);
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
}
