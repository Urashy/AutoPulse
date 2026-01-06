using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        public virtual async Task<bool> Exists(int idCompte1, int idCompte2,int idAnnonce)
        {
            var conversationsAnnonce = await context.Conversations
                 .Where(c => c.IdAnnonce == idAnnonce)
                 .Select(c => c.IdConversation)
                 .ToListAsync();

            if (!conversationsAnnonce.Any())
            {
                return false;
            }

            // Vérifier si les deux comptes participent à une même conversation de cette annonce
            var conversationCommune = await context.Set<APourConversation>()
                .Where(apc => conversationsAnnonce.Contains(apc.IdConversation))
                .GroupBy(apc => apc.IdConversation)
                .Where(g => g.Count() == 2 &&
                            g.Any(apc => apc.IdCompte == idCompte1) &&
                            g.Any(apc => apc.IdCompte == idCompte2))
                .AnyAsync();

            return conversationCommune;
        }
    }
}
