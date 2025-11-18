using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MarqueManager : ReadableManager<Modele>
    {
        public MarqueManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
