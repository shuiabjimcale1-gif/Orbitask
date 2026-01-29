using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Boards.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data.Boards
{
    public class TagData : ITagData
    {
        private readonly string _connectionString;

        public TagData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ============================================
        // GET SINGLE TAG
        // ============================================

        public async Task<Tag?> GetTag(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Tag>(
                "SELECT * FROM Tags WHERE Id = @Id",
                new { Id = tagId }
            );
        }

        // ============================================
        // GET TAGS FOR BOARD
        // ============================================

        public async Task<IEnumerable<Tag>> GetTagsForBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Tag>(
                "SELECT * FROM Tags WHERE BoardId = @BoardId ORDER BY Title",
                new { BoardId = boardId }
            );
        }

        // ============================================
        // INSERT TAG
        // ============================================

        public async Task<Tag> InsertTag(Tag tag)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Tags (Title, BoardId)
                OUTPUT INSERTED.Id, INSERTED.Title, INSERTED.BoardId
                VALUES (@Title, @BoardId);
            ";

            // ❌ REMOVED: WorkbenchId from INSERT

            return await connection.QuerySingleAsync<Tag>(sql, tag);
        }

        // ============================================
        // UPDATE TAG
        // ============================================

        public async Task<Tag?> UpdateTag(Tag tag)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE Tags
                SET Title = @Title
                WHERE Id = @Id;
                
                SELECT Id, Title, BoardId
                FROM Tags
                WHERE Id = @Id;
            ";

            // ❌ REMOVED: WorkbenchId from UPDATE
            // Note: BoardId is NOT updateable (tags can't move between boards)

            return await connection.QuerySingleOrDefaultAsync<Tag>(sql, tag);
        }

        // ============================================
        // DELETE TAG
        // ============================================

        public async Task<bool> DeleteTag(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var tx = connection.BeginTransaction();

            try
            {
                // 1) Delete TaskTags first (foreign key dependency)
                // ✅ Uses TagId (no change needed here - it's correct)
                await connection.ExecuteAsync(
                    "DELETE FROM TaskTags WHERE TagId = @Id;",
                    new { Id = tagId },
                    transaction: tx
                );

                // 2) Delete the tag
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Tags WHERE Id = @Id;",
                    new { Id = tagId },
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
        // EXISTENCE CHECKS
        // ============================================

        public async Task<bool> TagExists(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Tags WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = tagId }
            );
        }

        public async Task<bool> BoardExists(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Boards WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = boardId }
            );
        }

        // ============================================
        // BOARD LOOKUP
        // ============================================

        public async Task<int?> GetBoardIdForTag(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<int?>(
                "SELECT BoardId FROM Tags WHERE Id = @Id",
                new { Id = tagId }
            );
        }

        // ============================================
        // TENANCY HELPER (NEW)
        // ============================================

        /// <summary>
        /// Gets the WorkbenchId for a tag by JOINing through Board.
        /// Used for authorization checks.
        /// </summary>
        public async Task<int?> GetWorkbenchIdForTag(int tagId)
        {
            using var connection = new SqlConnection(_connectionString);

            // ✅ Go up the tree: Tag → Board → Workbench
            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM Tags t
                INNER JOIN Boards b ON t.BoardId = b.Id
                WHERE t.Id = @TagId",
                new { TagId = tagId }
            );
        }
    }
}