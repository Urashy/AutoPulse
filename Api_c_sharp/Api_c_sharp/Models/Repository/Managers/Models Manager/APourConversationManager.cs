namespace Api_c_sharp.Models.Repository.Managers.Models_Manager
{
    public class APourConversationManager : WriteableReadableManager<APourConversation>
    {
        public APourConversationManager(AutoPulseBdContext context) : base(context)
        {
        }
    }
}
