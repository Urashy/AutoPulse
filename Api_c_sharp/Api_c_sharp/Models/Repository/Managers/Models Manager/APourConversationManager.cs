using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class APourConversationManager : WriteableReadableManager<APourConversation>, IApourConversationRepository
    {
        public APourConversationManager(AutoPulseBdContext context) : base(context)
        { }

        public virtual async Task<APourConversation?> GetAPourConversationByIDS(int idCompte, int idConversation)
        {
            return await dbSet.FindAsync(idCompte, idConversation);
        }
    }
}
