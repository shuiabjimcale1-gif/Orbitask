using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;
using System.Security.Claims;

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
                "SELECT w.* FROM Workbenches w INNER JOIN WorkbenchMembers m ON w.Id = m.WorkbenchId WHERE m.UserId = @UserId ORDER BY w.Name;",
                new { UserId = userId }
            );
        }

        public async Task<Workbench?> InsertWorkbench(String userId, Workbench workbench)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                workbench.OwnerId = userId;

                var sql = @"
                    INSERT INTO Workbenches (OwnerId, Name)
                    OUTPUT INSERTED.Id, INSERTED.OwnerId, INSERTED.Name
                    VALUES (@OwnerId, @Name);";

                var created = await connection.QuerySingleAsync<Workbench>(
                    sql,
                    workbench,
                    transaction
                );

                var memberSql = @"
                    INSERT INTO WorkbenchMembers (WorkbenchId, UserId, Role)
                    OUTPUT INSERTED.WorkbenchId, INSERTED.UserId, INSERTED.Role
                    VALUES (@WorkbenchId, @UserId, @Role);";

                await connection.ExecuteAsync(
                    memberSql,
                    new
                    {
                        WorkbenchId = created.Id,
                        UserId = userId,
                        Role = WorkbenchMember.WorkbenchRole.Admin
                    },
                    transaction
                );

                // 3) Commit
                await transaction.CommitAsync();
                return created;
            }
            catch
            {
                await transaction.RollbackAsync();
                return null;
            }
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

                // 7. Delete Workbench Last
                var deleteWorkbenchSql =
                    "DELETE FROM Workbenches WHERE Id = @Id;";
                var rows = await connection.ExecuteAsync(deleteWorkbenchSql, new { Id = id }, transaction);

                await transaction.CommitAsync();

                return rows> 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
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

        public async Task<IEnumerable<string>> GetUsersForWorkbench(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @" SELECT UserId FROM WorkbenchMembers WHERE WorkbenchId = @WorkbenchId;";

            var users = await connection.QueryAsync<string>(sql, new { WorkbenchId = workbenchId });
            return users;
        }

        public async Task<bool> AddUserToWorkbench(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = "INSERT INTO WorkbenchMembers (WorkbenchId, UserId, Role) VALUES (@WorkbenchId, @UserId, @Role);";

            var rows = await connection.ExecuteAsync(sql, new
            {
                WorkbenchId = workbenchId,
                UserId = userId,
                Role = role
            });

            if (rows <= 0)
            {
                return false;
            }

            return true;
        }


        public async Task<bool> RemoveUserFromWorkbench(int workbenchId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @" DELETE FROM WorkbenchMembers WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId;";

            var rows = await connection.ExecuteAsync(sql, new
            {
                WorkbenchId = workbenchId,
                UserId = userId
            });

            if (rows <= 0)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateUserRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = "UPDATE WorkbenchMembers SET Role = @Role WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId;";

            var rows = await connection.ExecuteAsync(sql, new
            {
                WorkbenchId = workbenchId,
                UserId = userId,
                Role = role
            });

            if (rows <= 0)
            {
                return false;
            }

            return true;
        }

        public async Task<WorkbenchMember?> GetMembership(int workbenchId, string userId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"SELECT WorkbenchId, UserId, Role 
                FROM WorkbenchMembers 
                WHERE WorkbenchId = @WorkbenchId AND UserId = @UserId;";

            return await connection.QuerySingleOrDefaultAsync<WorkbenchMember>(sql, new
            {
                WorkbenchId = workbenchId,
                UserId = userId
            });
        }




    }
}
