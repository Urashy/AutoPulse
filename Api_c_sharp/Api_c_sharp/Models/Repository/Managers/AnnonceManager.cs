using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : BaseManager<Annonce,string>
    {
        private IQueryable<Annonce> ApplyIncludes()
        {
            return context.Set<Annonce>()
                .Include(p => p.MiseEnAvantAnnonceNav)
                .Include(p => p.CompteAnnonceNav)
                .Include(p => p.VoitureAnnonceNav);
        }
        public AnnonceManager(AutoPulseBdContext context) : base(context)
        { 
            
        }

        public override async Task<IEnumerable<Annonce>> GetAllAsync()
        {
            return await ApplyIncludes().ToListAsync();
        }

        public override Task<Annonce?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
