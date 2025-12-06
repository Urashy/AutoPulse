using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class VueManager : WriteableReadableManager<Vue>, IVueRepository
    {
        public VueManager(AutoPulseBdContext context) : base(context)
        { 
        }

        public async Task<Vue> GetVueByIdsAsync(int idCompte, int idAnnonce)
        {
            return await dbSet.FindAsync(idCompte, idAnnonce);
        }
    }
}
