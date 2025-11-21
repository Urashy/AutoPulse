using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore; 

namespace Api_c_sharp.Models.Repository.Managers
{
    public class MessageManager : WritableManager<Message>, ReadableRepository<Message>, IMessageRepository
    {
        public MessageManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Message>> GetMessagesByConversation(int conversationId)
        {
            return await dbSet.Where(m => m.IdConversation == conversationId).ToListAsync();
        }
    }
}
