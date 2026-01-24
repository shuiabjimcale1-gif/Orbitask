using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data
{
    public class BoardData : IBoardData
    {
        private readonly string _connectionString;

        public BoardData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ============================================
        // GET SINGLE BOARD
        // ============================================

        public async Task<Board?> GetBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Board>(
                "SELECT * FROM Boards WHERE Id = @Id",
                new { Id = boardId }
            );
        }

        // ============================================
        // GET BOARDS FOR WORKBENCH
        // ============================================

        public async Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Board>(
                "SELECT * FROM Boards WHERE WorkbenchId = @WorkbenchId ORDER BY Name",
                new { WorkbenchId = workbenchId }
            );
        }

        // ============================================
        // INSERT BOARD
        // ============================================

        public async Task<Board> InsertBoard(Board board)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                INSERT INTO Boards (Name, WorkbenchId)
                OUTPUT INSERTED.Id, INSERTED.WorkbenchId, INSERTED.Name
                VALUES (@Name, @WorkbenchId);
            ";

            return await connection.QuerySingleAsync<Board>(sql, board);
        }

        // ============================================
        // UPDATE BOARD
        // ============================================

        public async Task<bool> UpdateBoard(Board board)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                UPDATE Boards
                SET Name = @Name
                WHERE Id = @Id;
            ";

            var rows = await connection.ExecuteAsync(sql, board);
            return rows > 0;
        }

        // ============================================
        // DELETE BOARD
        // ============================================

        public async Task<bool> DeleteBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Delete TaskTags for tasks in this board
                // ✅ FIXED: Tasks → TaskItems, TaskId → TaskItemId
                await connection.ExecuteAsync(@"
                    DELETE TT 
                    FROM TaskTags TT 
                    INNER JOIN TaskItems T ON TT.TaskItemId = T.Id 
                    INNER JOIN Columns C ON T.ColumnId = C.Id
                    WHERE C.BoardId = @BoardId",
                    new { BoardId = boardId },
                    transaction
                );

                // 2. Delete Tags belonging to this board
                await connection.ExecuteAsync(
                    "DELETE FROM Tags WHERE BoardId = @BoardId;",
                    new { BoardId = boardId },
                    transaction
                );

                // 3. Delete TaskItems in this board
                // ✅ FIXED: Tasks → TaskItems
                await connection.ExecuteAsync(@"
                    DELETE FROM TaskItems 
                    WHERE ColumnId IN (SELECT Id FROM Columns WHERE BoardId = @BoardId)",
                    new { BoardId = boardId },
                    transaction
                );

                // 4. Delete Columns in this board
                await connection.ExecuteAsync(
                    "DELETE FROM Columns WHERE BoardId = @BoardId;",
                    new { BoardId = boardId },
                    transaction
                );

                // 5. Finally delete the board
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Boards WHERE Id = @BoardId;",
                    new { BoardId = boardId },
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

        // ============================================
        // EXISTENCE CHECKS
        // ============================================

        public async Task<bool> BoardExists(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Boards WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = boardId }
            );
        }

        public async Task<bool> WorkbenchExists(int workbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Workbenches WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = workbenchId }
            );
        }
    }
}