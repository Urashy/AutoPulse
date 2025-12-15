using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class OffreManager : WriteableReadableManager<Offre>
    {
        public OffreManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
