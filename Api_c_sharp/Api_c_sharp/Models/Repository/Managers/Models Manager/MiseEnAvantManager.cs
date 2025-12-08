using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class MiseEnAvantManager : ReadableManager<MiseEnAvant>
    {
        public MiseEnAvantManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
