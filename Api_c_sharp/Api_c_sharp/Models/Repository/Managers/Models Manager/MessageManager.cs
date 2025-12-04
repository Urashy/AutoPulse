using System.Data;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MessageManager : WriteableReadableManager<Message>, IMessageRepository
    {
        public MessageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId)
        {
            var messages = await dbSet
                .Where(m => m.IdConversation == conversationId && m.IdCompte != userId)
                .ToListAsync();

            foreach (var msg in messages)
                msg.EstLu = true;

            await context.SaveChangesAsync();

            return await dbSet
                .Include(m => m.MessageCompteNav)
                .Where(m => m.IdConversation == conversationId)
                .OrderBy(m => m.DateEnvoiMessage)
                .ToListAsync();
        }

        public async Task<int> GetUnreadMessageCount(int conversationId, int userId)
        {
            var list = dbSet.Where(m => m.IdConversation == conversationId && m.IdCompte !=  userId && m.EstLu == false).ToList();

            return list.Count;
        }
    }
}