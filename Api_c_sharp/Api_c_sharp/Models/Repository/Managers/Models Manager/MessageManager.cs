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

        /// <summary>
        /// Récupère les messages d'une conversation et les marque automatiquement comme lus
        /// via la fonction stockée en base de données
        /// </summary>
        public async Task<IEnumerable<Message>> GetMessagesByConversationAndMarkAsRead(int conversationId, int userId)
        {
            var connection = context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            // 1. Appeler la fonction pour marquer les messages comme lus
            using (var markCommand = connection.CreateCommand())
            {
                markCommand.CommandText = "SELECT sp_mark_messages_as_read(@p_conversation_id, @p_user_id)";
                markCommand.Parameters.Add(new NpgsqlParameter("@p_conversation_id", conversationId));
                markCommand.Parameters.Add(new NpgsqlParameter("@p_user_id", userId));

                await markCommand.ExecuteNonQueryAsync();
            }

            // 2. Récupérer les messages (maintenant marqués comme lus)
            return await dbSet
                .Include(m => m.MessageCompteNav)
                .Where(m => m.IdConversation == conversationId)
                .OrderBy(m => m.DateEnvoiMessage)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère le nombre de messages non lus pour une conversation via la fonction stockée
        /// </summary>
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