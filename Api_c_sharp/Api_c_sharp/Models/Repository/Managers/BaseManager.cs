using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Api_c_sharp.Models.Repository.Managers
{

        public abstract class BaseManager<TEntity> : IRepository<TEntity>
    where TEntity : class
        {
            protected readonly ProduitsbdContext context;
            protected readonly DbSet<TEntity> dbSet;

            public BaseManager(ProduitsbdContext context)
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

            
        }
    
}
