using Api_c_sharp.Models.Repository;
using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public abstract class SearchableManager<TEntity, TKey> : SearchableRepository<TEntity, TKey> where TEntity : class
    {
        protected readonly AutoPulseBdContext context;
        protected readonly DbSet<TEntity> dbSet;

        public SearchableManager(AutoPulseBdContext context)
        {
            this.context = context;
            this.dbSet = context?.Set<TEntity>();
        }

        public abstract Task<TEntity?> GetByNameAsync(TKey name);

    }
}