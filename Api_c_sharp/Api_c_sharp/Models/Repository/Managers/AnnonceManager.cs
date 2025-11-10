namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : WritableManager<Annonce>, SearchableManager<Annonce>, ManagerGenerique<Annonce>
    {
        public AnnonceManager(context context) : base(context)
        { 
            
        }
        public override async Task<IEnumerable<Annonce>> GetAllAsync()
        {
            return await dbSet
                .Include(m => m.Produits)
                .ToListAsync();
        }
        public override async Task<Annonce?> GetByIdAsync(int id)
        {
            return await dbSet
                .Include(m => m.Produits)
                .FirstOrDefaultAsync(m => m.IdMarque == id);
        }


    }
}
