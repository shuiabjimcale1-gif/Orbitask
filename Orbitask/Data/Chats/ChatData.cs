using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Chats.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data.Chats
{
    public class ChatData : IChatData
    {
        private readonly string _connectionString;

        public ChatData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<Chat>> GetChatsForUser(string userId, int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Chat>(@"
                SELECT c.Id, c.Type, c.WorkbenchId, c.Name, c.CreatedAt, c.LastMessageAt
                FROM Chats c
                INNER JOIN ChatUsers cu ON c.Id = cu.ChatId
                WHERE cu.UserId = @UserId AND c.WorkbenchId = @WorkbenchId
                ORDER BY c.LastMessageAt DESC, c.CreatedAt DESC",
                new { UserId = userId, WorkbenchId = workbenchId }
            );
        }

        public async Task<Chat?> GetChat(int chatId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Chat>(@"
                SELECT Id, Type, WorkbenchId, Name, CreatedAt, LastMessageAt
                FROM Chats
                WHERE Id = @Id",
                new { Id = chatId }
            );
        }

        public async Task<Chat> InsertChat(Chat chat)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleAsync<Chat>(@"
                INSERT INTO Chats (Type, WorkbenchId, Name, CreatedAt, LastMessageAt)
                OUTPUT INSERTED.Id, INSERTED.Type, INSERTED.WorkbenchId, 
                       INSERTED.Name, INSERTED.CreatedAt, INSERTED.LastMessageAt
                VALUES (@Type, @WorkbenchId, @Name, @CreatedAt, @LastMessageAt)",
                chat
            );
        }

        public async Task<Chat?> UpdateChat(Chat chat)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Chat>(@"
                UPDATE Chats
                SET Name = @Name
                WHERE Id = @Id;
                
                SELECT Id, Type, WorkbenchId, Name, CreatedAt, LastMessageAt
                FROM Chats
                WHERE Id = @Id",
                chat
            );
        }

        public async Task<bool> DeleteChat(int chatId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Delete messages first
                await connection.ExecuteAsync(
                    "DELETE FROM Messages WHERE ChatId = @ChatId",
                    new { ChatId = chatId },
                    transaction
                );

                // 2. Delete chat users
                await connection.ExecuteAsync(
                    "DELETE FROM ChatUsers WHERE ChatId = @ChatId",
                    new { ChatId = chatId },
                    transaction
                );

                // 3. Delete chat
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Chats WHERE Id = @Id",
                    new { Id = chatId },
                    transaction
                );

                await transaction.CommitAsync();
                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<int?> GetWorkbenchIdForChat(int chatId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT WorkbenchId FROM Chats WHERE Id = @Id",
                new { Id = chatId }
            );
        }

        public async Task UpdateLastMessageAt(int chatId, DateTime timestamp)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
                UPDATE Chats
                SET LastMessageAt = @Timestamp
                WHERE Id = @ChatId",
                new { ChatId = chatId, Timestamp = timestamp }
            );
        }

        public async Task<bool> ChatExists(int chatId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (SELECT 1 FROM Chats WHERE Id = @Id) 
                THEN 1 ELSE 0 END",
                new { Id = chatId }
            );
        }
    }
}