using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;
namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceReadableManager : SearchableReadableManager<Annonce>, WritableManager<Annonce>
    {
        public AnnonceReadableManager(AutoPulseBdContext context) : base(context)
        { 
            
        }
    }
}
