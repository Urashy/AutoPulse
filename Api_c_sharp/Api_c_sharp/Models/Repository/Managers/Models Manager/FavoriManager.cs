namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class FavoriManager : WriteableReadableManager<Favori>
    {
        public FavoriManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
