using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class CompteManager : SearchableManager<Compte,string>, WritableRepository<Compte>
    {
        public CompteManager(AutoPulseBdContext context) : base(context)
        {
        }

        public virtual async Task<Compte> AddAsync(Compte entity)
        {
            await dbSet.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task DeleteAsync(Compte entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public override Task<Compte?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public virtual async Task UpdateAsync(Compte entityToUpdate, Compte entity)
        {
            context.Entry(entityToUpdate).CurrentValues.SetValues(entity);
            await context.SaveChangesAsync();
        }
    }
}
