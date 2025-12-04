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

        public async Task<IEnumerable<Message>> GetMessagesByConversation(int conversationId)
        {
            return await dbSet.Include(m=> m.MessageCompteNav).Where(m => m.IdConversation == conversationId).OrderBy(m => m.DateEnvoiMessage).ToListAsync();
        }

        public async Task<int> GetUnreadMessageCount(int conversationId, int userId)
        {
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT sp_count_unread_messages(@p_conversation_id, @p_user_id)";
            command.Parameters.Add(new NpgsqlParameter("@p_conversation_id", conversationId));
            command.Parameters.Add(new NpgsqlParameter("@p_user_id", userId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}
