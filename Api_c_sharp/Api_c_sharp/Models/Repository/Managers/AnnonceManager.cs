using Api_c_sharp.Models.Repository.Interfaces;
using System.Collections.Generic;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class AnnonceManager : BaseManager<Annonce,string>
    {
        public AnnonceManager(AutoPulseBdContext context) : base(context)
        { 
            
        }

        public override Task<Annonce?> GetByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
