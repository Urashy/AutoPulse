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

        public virtual async Task<IEnumerable<Conversation>> GetConversationsByCompteID(int compteId, int annonceId= 0)
        {
            var query = dbSet
                .Include(c => c.ApourConversations)
                    .ThenInclude(apc => apc.APourConversationCompteNav)
                .Include(c => c.Messages)
                .Include(c => c.AnnonceConversationNav)
                .Where(c => c.ApourConversations.Any(ac => ac.IdCompte == compteId));

            if (annonceId != 0)
            {
                query = query
                    .Where(c => c.IdAnnonce == annonceId)
                    .OrderBy(c => c.IdAnnonce)
                    .ThenByDescending(c => c.DateDernierMessage);
            }
            else
            {
                query = query.OrderByDescending(c => c.DateDernierMessage);
            }

            return await query.ToListAsync();
        }

        public virtual async Task<Conversation> PostComplet(Conversation conversation, string contenumessage, int idcompteenvoi, int idcompterecoi)
        {
            var existingConversation = await dbSet
                .Include(c => c.ApourConversations)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.IdAnnonce == conversation.IdAnnonce &&
                    c.ApourConversations.Any(ac => ac.IdCompte == idcompteenvoi) &&
                    c.ApourConversations.Any(ac => ac.IdCompte == idcompterecoi));

            if (existingConversation != null)
            {
                var newMessage = new Message
                {
                    IdConversation = existingConversation.IdConversation,
                    EstLu = false,
                    DateEnvoiMessage = DateTime.UtcNow,
                    ContenuMessage = contenumessage,
                    IdCompte = idcompteenvoi
                };

                context.Messages.Add(newMessage);

                existingConversation.DateDernierMessage = DateTime.UtcNow;

                await context.SaveChangesAsync();
                return existingConversation;
            }

            await dbSet.AddAsync(conversation);
            await context.SaveChangesAsync();

            var participant1 = new APourConversation
            {
                IdCompte = idcompteenvoi,
                IdConversation = conversation.IdConversation
            };

            var participant2 = new APourConversation
            {
                IdCompte = idcompterecoi,
                IdConversation = conversation.IdConversation
            };

            context.APourConversations.Add(participant1);
            context.APourConversations.Add(participant2);

            var firstMessage = new Message
            {
                IdConversation = conversation.IdConversation,
                EstLu = false,
                DateEnvoiMessage = DateTime.UtcNow,
                ContenuMessage = contenumessage,
                IdCompte = idcompteenvoi
            };

            context.Messages.Add(firstMessage);

            await context.SaveChangesAsync();

            return conversation;
        }
    }
}
