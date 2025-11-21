using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class ConversationManager : WritableManager<Conversation>, ReadableRepository<Conversation>
    {
        public ConversationManager(AutoPulseBdContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Conversation>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<Conversation?> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }
    }
}
