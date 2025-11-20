using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MotriciteManager : ReadableManager<Motricite>
    {
        public MotriciteManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
