using Api_c_sharp.Models.Entity;
using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MessageManager : WriteableReadableManager<Message>, IMessageRepository
    {
        public MessageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversation(int conversationId)
        {
            return await dbSet.Where(m => m.IdConversation == conversationId).OrderBy(m => m.DateEnvoiMessage).ToListAsync();
        }
    }
}
