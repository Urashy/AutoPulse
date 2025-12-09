using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class BoiteDeVitesseManager : ReadableManager<BoiteDeVitesse>
    {
        public BoiteDeVitesseManager(AutoPulseBdContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<BoiteDeVitesse>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.LibelleBoite).ToListAsync();
        }
    }
}
