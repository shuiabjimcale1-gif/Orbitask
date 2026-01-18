using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Models;
using Orbitask.Data.Interfaces;

namespace Orbitask.Data
{
    public class WorkbenchData : IWorkbenchData
    {
        private readonly string _connectionString;

        public WorkbenchData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<Workbench?> GetWorkbench(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Workbench>(
                "SELECT * FROM Workbenches WHERE Id = @Id",
                new { Id = id }
            );
        }

        public async Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Workbench>(
                "SELECT * FROM Workbenches WHERE OwnerId = @UserId ORDER BY Name",
                new { UserId = userId }
            );
        }

        public async Task<Workbench> InsertWorkbench(Workbench workbench)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql =
                "INSERT INTO Workbenches (OwnerId, Name) " +
                "OUTPUT INSERTED.Id, INSERTED.OwnerId, INSERTED.Name " +
                "VALUES (@OwnerId, @Name);";

            return await connection.QuerySingleAsync<Workbench>(sql, workbench);
        }

        public async Task<bool> UpdateWorkbench(Workbench workbench)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql =
                "UPDATE Workbenches SET Name = @Name WHERE Id = @Id;";

            var rows = await connection.ExecuteAsync(sql, workbench);
            return rows > 0;
        }

        public async Task<bool> DeleteWorkbench(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Delete TaskTags using TaskIds under this Workbench
                var deleteTaskTagsSql =
                    @"DELETE TT FROM TaskTags TT INNER JOIN Tasks T ON TT.TaskId = T.Id WHERE T.WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteTaskTagsSql, new { Id = id }, transaction);

                // 2. Delete Tags
                var deleteTagsSql =
                    "DELETE FROM Tags WHERE WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteTagsSql, new { Id = id }, transaction);

                // 3. Delete Tasks
                var deleteTasksSql =
                    "DELETE FROM Tasks WHERE WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteTasksSql, new { Id = id }, transaction);

                // 4. Delete Columns
                var deleteColumnsSql =
                    "DELETE FROM Columns WHERE WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteColumnsSql, new { Id = id }, transaction);

                // 5. Delete Boards
                var deleteBoardsSql =
                    "DELETE FROM Boards WHERE WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteBoardsSql, new { Id = id }, transaction);

                // 6. Delete Workbench Members (if exists)
                var deleteMembersSql =
                    "DELETE FROM WorkbenchMembers WHERE WorkbenchId = @Id;";
                await connection.ExecuteAsync(deleteMembersSql, new { Id = id }, transaction);

                // 7. Delete Workbench
                var deleteWorkbenchSql =
                    "DELETE FROM Workbenches WHERE Id = @Id;";
                var rows = await connection.ExecuteAsync(deleteWorkbenchSql, new { Id = id }, transaction);

                await transaction.CommitAsync();

                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> WorkbenchExists(int id)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Workbenches WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = id }
            );
        }

        public async Task<bool> UserOwnsWorkbench(int id, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Workbenches WHERE Id = @Id AND OwnerId = @UserId) THEN 1 ELSE 0 END",
                new { Id = id, UserId = userId }
            );
        }
    }
}
