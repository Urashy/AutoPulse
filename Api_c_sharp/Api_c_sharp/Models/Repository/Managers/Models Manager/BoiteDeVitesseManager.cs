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
    }
}
