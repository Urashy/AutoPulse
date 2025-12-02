using Api_c_sharp.Models.Entity;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class APourConversationManager : WriteableReadableManager<APourConversation>
    {
        public APourConversationManager(AutoPulseBdContext context) : base(context)
        { }

                public async Task<APourConversation?> GetByIdsAsync(int idCompte, int idConversation)
            {
                return await context.Set<APourConversation>()
                    .FindAsync(idCompte, idConversation);
            }
    }
}
