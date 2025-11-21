using Api_c_sharp.Models.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class ConversationManager : WriteableReadableManager<Conversation>
    {
        public ConversationManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
