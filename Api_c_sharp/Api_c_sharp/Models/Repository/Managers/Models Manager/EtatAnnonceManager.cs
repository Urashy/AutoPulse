using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class EtatAnnonceManager : ReadableManager<EtatAnnonce>
    {
        public EtatAnnonceManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
