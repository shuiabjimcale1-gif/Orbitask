using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Boards.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data.Boards
{
    public class TaskItemData : ITaskItemData
    {
        private readonly string _connectionString;

        public TaskItemData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ============================================
        // GET SINGLE TASK
        // ============================================

        public async Task<TaskItem?> GetTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<TaskItem>(
                "SELECT * FROM TaskItems WHERE Id = @Id",
                new { Id = taskId }
            );
        }

        // ============================================
        // GET TASKS FOR COLUMN
        // ============================================

        public async Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<TaskItem>(
                "SELECT * FROM TaskItems WHERE ColumnId = @ColumnId ORDER BY Position",
                new { ColumnId = columnId }
            );
        }

        // ============================================
        // INSERT TASK
        // ============================================

        public async Task<TaskItem> InsertTask(TaskItem task)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO TaskItems (Title, Description, Position, ColumnId)
                OUTPUT 
                    INSERTED.Id,
                    INSERTED.Title,
                    INSERTED.Description,
                    INSERTED.Position,
                    INSERTED.ColumnId,
                    INSERTED.CreatedOn,
                    INSERTED.DueDate
                VALUES (@Title, @Description, @Position, @ColumnId);
            ";

            // ❌ REMOVED: BoardId and WorkbenchId from INSERT

            return await connection.QuerySingleAsync<TaskItem>(sql, task);
        }

        // ============================================
        // UPDATE TASK
        // ============================================

        public async Task<bool> UpdateTask(TaskItem task)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE TaskItems
                SET 
                    Title = @Title,
                    Description = @Description,
                    Position = @Position,
                    ColumnId = @ColumnId,
                    DueDate = @DueDate
                WHERE Id = @Id;
            ";

            // ❌ REMOVED: BoardId and WorkbenchId from UPDATE

            var rows = await connection.ExecuteAsync(sql, task);
            return rows > 0;
        }

        // ============================================
        // DELETE TASK
        // ============================================

        public async Task<bool> DeleteTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var tx = connection.BeginTransaction();

            try
            {
                // 1) Delete TaskTags first (foreign key dependency)
                // ✅ FIXED: TaskId → TaskItemId
                await connection.ExecuteAsync(
                    "DELETE FROM TaskTags WHERE TaskItemId = @Id;",
                    new { Id = taskId },
                    transaction: tx
                );

                // 2) Delete the TaskItem
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

        // ============================================
        // ATTACH TAG TO TASK
        // ============================================

        public async Task<bool> AttachTag(int taskId, int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ FIXED: TaskId → TaskItemId
            var sql = @"
                -- Only insert if not already exists
                IF NOT EXISTS (
                    SELECT 1 FROM TaskTags 
                    WHERE TaskItemId = @TaskItemId AND TagId = @TagId
                )
                BEGIN
                    INSERT INTO TaskTags (TaskItemId, TagId)
                    VALUES (@TaskItemId, @TagId);
                END
            ";

            await connection.ExecuteAsync(sql, new
            {
                TaskItemId = taskId,  // ✅ Renamed parameter
                TagId = tagId
            });

            return true;
        }

        // ============================================
        // REMOVE TAG FROM TASK
        // ============================================

        public async Task<bool> RemoveTag(int taskId, int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ FIXED: TaskId → TaskItemId
            var rows = await connection.ExecuteAsync(
                "DELETE FROM TaskTags WHERE TaskItemId = @TaskItemId AND TagId = @TagId",
                new
                {
                    TaskItemId = taskId,  // ✅ Renamed parameter
                    TagId = tagId
                }
            );

            return rows > 0;
        }

        // ============================================
        // EXISTENCE CHECKS
        // ============================================

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

        // ============================================
        // TENANCY HELPER (NEW)
        // ============================================

        /// <summary>
        /// Gets the WorkbenchId for a task by JOINing through Column and Board.
        /// This is used for authorization checks in the controller.
        /// </summary>
        public async Task<int?> GetWorkbenchIdForTask(int taskId)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ NEW: Join through hierarchy to get WorkbenchId
            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM TaskItems t
                INNER JOIN Columns c ON t.ColumnId = c.Id
                INNER JOIN Boards b ON c.BoardId = b.Id
                WHERE t.Id = @TaskId",
                new { TaskId = taskId }
            );
        }
    }
}