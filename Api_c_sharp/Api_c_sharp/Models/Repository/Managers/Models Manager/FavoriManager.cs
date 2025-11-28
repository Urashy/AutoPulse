namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FavoriManager : WriteableReadableManager<Favori>
    {
        public FavoriManager(AutoPulseBdContext context) : base(context)
        {

        }
        public async Task<Favori?> GetByIdAsync(int idCompte, int idAnnonce)
        {
            return await dbSet.FindAsync(idCompte, idAnnonce);
        }
    }
}
