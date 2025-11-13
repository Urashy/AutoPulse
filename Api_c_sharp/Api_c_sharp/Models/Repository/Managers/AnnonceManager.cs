using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : ReadableManager<Annonce>, SearchableRepository<Annonce, string> // ReadbleSearchableManager
    {
        public AnnonceManager(AutoPulseBdContext context) : base(context)
        { 
            
        }
    }
}
