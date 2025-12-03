using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class ConversationManager : WriteableReadableManager<Conversation>, IConversationRepository
    {
        public ConversationManager(AutoPulseBdContext context) : base(context)
        {
        }
        public override async Task<IEnumerable<Conversation>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.DateDernierMessage).ToListAsync();
        }

        public async Task<IEnumerable<Conversation>> GetConversationsByCompteID(int compteId)
        {
            return await dbSet
                .Include(c => c.ApourConversations)
                .ThenInclude(apc => apc.APourConversationCompteNav)
                .ThenInclude(c => c.Images)
                .Include(c => c.Messages)
                .Include(c => c.AnnonceConversationNav)
                .Where(c => c.ApourConversations.Any(ac => ac.IdCompte == compteId))
                .OrderByDescending(c => c.DateDernierMessage)
                .ToListAsync();
        }
    }
}
