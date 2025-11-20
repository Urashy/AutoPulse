using Api_c_sharp.Models.Repository.Interfaces;
using Api_c_sharp.Models.Repository.Managers;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MoyenPaiementManager : ReadableManager<MoyenPaiement>
    {
        public MoyenPaiementManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
