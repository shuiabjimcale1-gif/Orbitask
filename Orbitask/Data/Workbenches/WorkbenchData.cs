using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Workbenches.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data.Workbenches
{
    public class WorkbenchData : IWorkbenchData
    {
        private readonly string _connectionString;

        public WorkbenchData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<Workbench?> GetWorkbench(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Workbench>(
                "SELECT * FROM Workbenches WHERE Id = @Id",
                new { Id = workbenchId }
            );
        }

        public async Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<Workbench>(@"
                SELECT w.*
                FROM Workbenches w
                INNER JOIN WorkbenchMembers wm ON w.Id = wm.WorkbenchId
                WHERE wm.UserId = @UserId
                ORDER BY w.Name",
                new { UserId = userId }
            );
        }

        public async Task<Workbench> InsertWorkbench(Workbench workbench)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleAsync<Workbench>(@"
                INSERT INTO Workbenches (Name)
                OUTPUT INSERTED.Id, INSERTED.Name
                VALUES (@Name);",
                workbench
            );
        }

        public async Task<Workbench?> UpdateWorkbench(Workbench workbench)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<Workbench>(@"
                UPDATE Workbenches SET Name = @Name WHERE Id = @Id;
                SELECT Id, Name FROM Workbenches WHERE Id = @Id;",
                workbench
            );
        }

        public async Task<bool> DeleteWorkbench(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. TaskTags
                await connection.ExecuteAsync(@"
                    DELETE TT FROM TaskTags TT 
                    INNER JOIN TaskItems T ON TT.TaskItemId = T.Id 
                    INNER JOIN Columns C ON T.ColumnId = C.Id
                    INNER JOIN Boards B ON C.BoardId = B.Id
                    WHERE B.WorkbenchId = @Id",
                    new { Id = workbenchId }, transaction
                );

                // 2. Tags
                await connection.ExecuteAsync(
                    "DELETE FROM Tags WHERE BoardId IN (SELECT Id FROM Boards WHERE WorkbenchId = @Id)",
                    new { Id = workbenchId }, transaction
                );

                // 3. TaskItems
                await connection.ExecuteAsync(@"
                    DELETE FROM TaskItems 
                    WHERE ColumnId IN (
                        SELECT C.Id FROM Columns C
                        INNER JOIN Boards B ON C.BoardId = B.Id
                        WHERE B.WorkbenchId = @Id
                    )",
                    new { Id = workbenchId }, transaction
                );

                // 4. Columns
                await connection.ExecuteAsync(
                    "DELETE FROM Columns WHERE BoardId IN (SELECT Id FROM Boards WHERE WorkbenchId = @Id)",
                    new { Id = workbenchId }, transaction
                );

                // 5. Boards
                await connection.ExecuteAsync(
                    "DELETE FROM Boards WHERE WorkbenchId = @Id;",
                    new { Id = workbenchId }, transaction
                );

                // 6. Members
                await connection.ExecuteAsync(
                    "DELETE FROM WorkbenchMembers WHERE WorkbenchId = @Id;",
                    new { Id = workbenchId }, transaction
                );

                // 7. Workbench
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Workbenches WHERE Id = @Id;",
                    new { Id = workbenchId }, transaction
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

        // 🎯 THE "TWO BIRDS ONE STONE" METHOD
        public async Task<WorkbenchMember?> GetMembership(int workbenchId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<WorkbenchMember>(@"
                SELECT WorkbenchId, UserId, Role
                FROM WorkbenchMembers
                WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId",
                new { WorkbenchId = workbenchId, UserId = userId }
            );
        }

        public async Task<IEnumerable<WorkbenchMember>> GetMembersForWorkbench(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.QueryAsync<WorkbenchMember>(@"
                SELECT WorkbenchId, UserId, Role
                FROM WorkbenchMembers
                WHERE WorkbenchId = @WorkbenchId
                ORDER BY Role, UserId",
                new { WorkbenchId = workbenchId }
            );
        }

        public async Task<bool> AddMember(WorkbenchMember member)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync(@"
                IF NOT EXISTS (SELECT 1 FROM WorkbenchMembers 
                               WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId)
                BEGIN
                    INSERT INTO WorkbenchMembers (WorkbenchId, UserId, Role)
                    VALUES (@WorkbenchId, @UserId, @Role);
                END",
                member
            );
            return true;
        }

        public async Task<bool> UpdateMemberRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            using var connection = new SqlConnection(_connectionString);
            var rows = await connection.ExecuteAsync(@"
                UPDATE WorkbenchMembers SET Role = @Role
                WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId",
                new { WorkbenchId = workbenchId, UserId = userId, Role = role }
            );
            return rows > 0;
        }

        public async Task<bool> RemoveMember(int workbenchId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            var rows = await connection.ExecuteAsync(@"
                DELETE FROM WorkbenchMembers
                WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId",
                new { WorkbenchId = workbenchId, UserId = userId }
            );
            return rows > 0;
        }

        public async Task<bool> WorkbenchExists(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Workbenches WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = workbenchId }
            );
        }

        public async Task<bool> UserExists(string userId)
        {
            using var connection = new SqlConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = userId }
            );
        }
    }
}