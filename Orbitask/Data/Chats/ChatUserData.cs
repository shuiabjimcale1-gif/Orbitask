using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data
{
    public class ChatUserData : IChatUserData
    {
        private readonly string _connectionString;

        public ChatUserData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<ChatUser>> GetMembersForChat(int chatId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<ChatUser>(@"
                SELECT ChatId, UserId, Role, JoinedAt
                FROM ChatUsers
                WHERE ChatId = @ChatId
                ORDER BY JoinedAt",
                new { ChatId = chatId }
            );
        }

        public async Task<ChatUser?> GetChatMembership(int chatId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<ChatUser>(@"
                SELECT ChatId, UserId, Role, JoinedAt
                FROM ChatUsers
                WHERE ChatId = @ChatId AND UserId = @UserId",
                new { ChatId = chatId, UserId = userId }
            );
        }

        public async Task<bool> AddMember(ChatUser member)
        {
            using var connection = new SqlConnection(_connectionString);

            await connection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM ChatUsers WHERE ChatId = @ChatId AND UserId = @UserId)
                BEGIN
                    INSERT INTO ChatUsers (ChatId, UserId, Role, JoinedAt)
                    VALUES (@ChatId, @UserId, @Role, @JoinedAt)
                END",
                member
            );

            return true;
        }

        public async Task<bool> UpdateMemberRole(int chatId, string userId, ChatUser.ChatRole role)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
                UPDATE ChatUsers
                SET Role = @Role
                WHERE ChatId = @ChatId AND UserId = @UserId",
                new { ChatId = chatId, UserId = userId, Role = role }
            );

            return rows > 0;
        }

        public async Task<bool> RemoveMember(int chatId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(@"
                DELETE FROM ChatUsers
                WHERE ChatId = @ChatId AND UserId = @UserId",
                new { ChatId = chatId, UserId = userId }
            );

            return rows > 0;
        }

        public async Task<bool> IsMember(int chatId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(@"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM ChatUsers 
                    WHERE ChatId = @ChatId AND UserId = @UserId
                ) THEN 1 ELSE 0 END",
                new { ChatId = chatId, UserId = userId }
            );
        }
    }
}