namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class APourCouleurManager : WriteableReadableManager<APourCouleur>
    {
        public APourCouleurManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
