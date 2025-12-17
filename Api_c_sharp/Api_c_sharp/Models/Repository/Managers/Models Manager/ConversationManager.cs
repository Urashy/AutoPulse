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

        public virtual async Task<IEnumerable<Conversation>> GetConversationsByCompteID(int compteId)
        {
            return await dbSet
                .Include(c => c.ApourConversations)
                .ThenInclude(apc => apc.APourConversationCompteNav)
                .Include(c => c.Messages)
                .Include(c => c.AnnonceConversationNav)
                .Where(c => c.ApourConversations.Any(ac => ac.IdCompte == compteId))
                .OrderByDescending(c => c.DateDernierMessage)
                .ToListAsync();
        }

        public virtual async Task<Conversation> PostComplet(Conversation conversation, string contenumessage, int idcompteenvoi, int idcompterecoi)
        {
            await dbSet.AddAsync(conversation);
            await context.SaveChangesAsync();
            
            context.APourConversations.Add(new APourConversation { IdCompte = idcompteenvoi, IdConversation = conversation.IdConversation });
            context.APourConversations.Add(new APourConversation { IdCompte = idcompterecoi, IdConversation = conversation.IdConversation });

            context.Messages.Add(new Message { IdConversation = conversation.IdConversation, EstLu = false, DateEnvoiMessage = DateTime.UtcNow, ContenuMessage = contenumessage, IdCompte = idcompteenvoi });
            await context.SaveChangesAsync();
            return conversation;
        }
    }
}
