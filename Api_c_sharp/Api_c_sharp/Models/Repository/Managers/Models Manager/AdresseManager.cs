using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AdresseManager : WriteableReadableManager<Adresse>
    {
        public AdresseManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
