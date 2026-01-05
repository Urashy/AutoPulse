using System.Data;
using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class MessageManager : WriteableReadableManager<Message>, IMessageRepository
    {
        public MessageManager(AutoPulseBdContext context) : base(context)
        {
        }
        
        public virtual async Task<IEnumerable<Message>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId)
        {
            // Récupérer les messages NON LUS de l'autre utilisateur
            var messagesToMark = await dbSet
                .Where(m => m.IdConversation == conversationId && m.IdCompte != userId)
                .ToListAsync();

            // Les marquer comme lus
            foreach (var msg in messagesToMark)
            {
                msg.EstLu = true;
            }

            await context.SaveChangesAsync();

            // ✅ Récupérer la liste complète des messages avec les pièces jointes
            var allMessages = await dbSet
                .Include(m => m.MessageCompteNav)
                .Include(m => m.Offres)
                .Include(m => m.PiecesJointes) 
                .Where(m => m.IdConversation == conversationId)
                .OrderBy(m => m.DateEnvoiMessage)
                .ToListAsync();

            return allMessages;
        }

        public virtual async Task<int> GetUnreadMessageCount(int conversationId, int userId)
        {
            var list = await dbSet.Where(m => m.IdConversation == conversationId && m.IdCompte !=  userId && m.EstLu == false).ToListAsync();

            return list.Count;
        }
    }
}