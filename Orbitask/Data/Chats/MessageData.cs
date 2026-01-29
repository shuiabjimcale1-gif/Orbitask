using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data
{
    public class MessageData : IMessageData
    {
        private readonly string _connectionString;

        public MessageData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Message>> GetMessagesForChat(int chatId, int limit = 50)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Message>(@"
                SELECT TOP(@Limit) Id, ChatId, UserId, Content, CreatedAt
                FROM Messages
                WHERE ChatId = @ChatId
                ORDER BY CreatedAt DESC",
                new { ChatId = chatId, Limit = limit }
            );
        }

        public async Task<Message?> GetMessage(int messageId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Message>(@"
                SELECT Id, ChatId, UserId, Content, CreatedAt
                FROM Messages
                WHERE Id = @Id",
                new { Id = messageId }
            );
        }

        public async Task<Message> InsertMessage(Message message)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleAsync<Message>(@"
                INSERT INTO Messages (ChatId, UserId, Content, CreatedAt)
                OUTPUT INSERTED.Id, INSERTED.ChatId, INSERTED.UserId, 
                       INSERTED.Content, INSERTED.CreatedAt
                VALUES (@ChatId, @UserId, @Content, @CreatedAt)",
                message
            );
        }

        public async Task<Message?> UpdateMessage(Message message)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Message>(@"
                UPDATE Messages
                SET Content = @Content
                WHERE Id = @Id;
                
                SELECT Id, ChatId, UserId, Content, CreatedAt
                FROM Messages
                WHERE Id = @Id",
                message
            );
        }

        public async Task<bool> DeleteMessage(int messageId)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
                DELETE FROM Messages WHERE Id = @Id",
                new { Id = messageId }
            );

            return rows > 0;
        }

        public async Task<int?> GetChatIdForMessage(int messageId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT ChatId FROM Messages WHERE Id = @Id",
                new { Id = messageId }
            );
        }
    }
}