namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FavoriManager : WriteableReadableManager<Favori>
    {
        public FavoriManager(AutoPulseBdContext context) : base(context)
        {

        }
        public override async Task DeleteAsync(Favori entity)
        {
            dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<Favori?> GetByIdAsync(int idCompte, int idAnnonce)
        {
            return await dbSet.FindAsync(idCompte, idAnnonce);
        }
    }
}
