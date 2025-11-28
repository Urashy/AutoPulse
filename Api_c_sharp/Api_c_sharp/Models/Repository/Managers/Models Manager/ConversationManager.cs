using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class ConversationManager : WriteableReadableManager<Conversation>
    {
        public ConversationManager(AutoPulseBdContext context) : base(context)
        {
        }
        public override async Task<IEnumerable<Conversation>> GetAllAsync()
        {
            return await dbSet.OrderBy(s => s.DateDernierMessage).ToListAsync();
        }
    }
}
