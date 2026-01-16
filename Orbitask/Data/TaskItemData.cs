using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data
{
    public class TaskItemData : ITaskItemData
    {
        private readonly string _connectionString;

        public TaskItemData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<TaskItem?> GetTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<TaskItem>(
                "SELECT * FROM TaskItems WHERE Id = @Id",
                new { Id = taskId }
            );
        }

        public async Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<TaskItem>(
                "SELECT * FROM TaskItems WHERE ColumnId = @ColumnId ORDER BY Position",
                new { ColumnId = columnId }
            );
        }

        public async Task<TaskItem> InsertTask(TaskItem task)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO TaskItems (Title, Description, Position, ColumnId, BoardId)
                        OUTPUT INSERTED.Id,
                        INSERTED.Title,
                        INSERTED.Description,
                        INSERTED.Position,
                        INSERTED.ColumnId,
                        INSERTED.BoardId,
                        INSERTED.CreatedOn,
                        INSERTED.DueDate
                        VALUES (@Title, @Description, @Position, @ColumnId, @BoardId);
                        ";

            return await connection.QuerySingleAsync<TaskItem>(sql, task);
        }


        public async Task<bool> UpdateTask(TaskItem task)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE TaskItems
                SET Title = @Title,
                    Description = @Description,
                    Position = @Position,
                    ColumnId = @ColumnId,
                    BoardId = @BoardId
                WHERE Id = @Id;";

            var rows = await connection.ExecuteAsync(sql, task);
            return rows > 0;
        }

        
        // DELETE TASK
        
        public async Task<bool> DeleteTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var tx = connection.BeginTransaction();

            try
            {
                
                await connection.ExecuteAsync(
                    "DELETE FROM TaskTags WHERE TaskId = @Id;",
                    new { Id = taskId },
                    transaction: tx
                );

                var rows = await connection.ExecuteAsync(
                    "DELETE FROM TaskItems WHERE Id = @Id;",
                    new { Id = taskId },
                    transaction: tx
                );

                tx.Commit();
                return rows > 0; 
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }



        
        // ATTACH TAG
        
        public async Task<bool> AttachTag(int taskId, int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
            IF NOT EXISTS (
                SELECT 1 FROM TaskTags WHERE TaskId = @TaskId AND TagId = @TagId
            )
            INSERT INTO TaskTags (TaskId, TagId)
            VALUES (@TaskId, @TagId);
        ";

            await connection.ExecuteAsync(sql, new { TaskId = taskId, TagId = tagId });
            return true;
        }

        
        // REMOVE TAG
        
        public async Task<bool> RemoveTag(int taskId, int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            var rows = await connection.ExecuteAsync(
                "DELETE FROM TaskTags WHERE TaskId = @TaskId AND TagId = @TagId",
                new { TaskId = taskId, TagId = tagId }
            );

            return rows > 0;
        }

        
        // EXISTENCE CHECKS
        
        public async Task<bool> TaskExists(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM TaskItems WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = taskId }
            );
        }

        public async Task<bool> ColumnExists(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Columns WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = columnId }
            );
        }

        public async Task<bool> TagExists(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Tags WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = tagId }
            );
        }

        
        // BOARD LOOKUPS
        
        public async Task<int?> GetBoardIdForTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int?>(
                "SELECT BoardId FROM TaskItems WHERE Id = @Id",
                new { Id = taskId }
            );
        }

        public async Task<int?> GetBoardIdForColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int?>(
                "SELECT BoardId FROM Columns WHERE Id = @Id",
                new { Id = columnId }
            );
        }

        public async Task<int?> GetBoardIdForTag(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int?>(
                "SELECT BoardId FROM Tags WHERE Id = @Id",
                new { Id = tagId }
            );
        }
    }

}
