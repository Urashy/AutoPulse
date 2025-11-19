namespace Api_c_sharp.Models.Repository.Managers
{
    public class CouleurManager : ReadableManager<Couleur>
    {
        public CouleurManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
