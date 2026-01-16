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

        public async Task<Board?> GetBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Board>(
                "SELECT * FROM Boards WHERE Id = @Id",
                new { Id = boardId }
            );
        }

        public async Task<IEnumerable<Board>> GetBoardsForWorkbench(int WorkbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Board>(
                "SELECT * FROM Boards WHERE WorkbenchId = @WorkbenchId ORDER BY Name",
                new { WorkbenchId = WorkbenchId }
            );
        }

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

        public async Task<bool> DeleteBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Delete TaskTags for tasks in this board
                var deleteTaskTagsSql =
                    "DELETE TT FROM TaskTags TT INNER JOIN Tasks T ON TT.TaskId = T.Id WHERE T.BoardId = @BoardId;";

                await connection.ExecuteAsync(deleteTaskTagsSql, new { BoardId = boardId }, transaction);

                // Delete Tags belonging to this board
                var deleteTagsSql =
                    "DELETE FROM Tags WHERE BoardId = @BoardId;";

                await connection.ExecuteAsync(deleteTagsSql, new { BoardId = boardId }, transaction);

                // Delete Tasks belonging to this board
                var deleteTasksSql =
                    "DELETE FROM Tasks WHERE BoardId = @BoardId;";

                await connection.ExecuteAsync(deleteTasksSql, new { BoardId = boardId }, transaction);

                // Finally delete the board
                var deleteBoardSql =
                    "DELETE FROM Boards WHERE Id = @BoardId;";

                var rows = await connection.ExecuteAsync(deleteBoardSql, new { BoardId = boardId }, transaction);

                await transaction.CommitAsync();

                return rows > 0;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }



        public async Task<bool> BoardExists(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Boards WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = boardId }
            );
        }

        public async Task<bool> WorkbenchExists(int WorkbenchId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Workbenches WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = WorkbenchId }
            );
        }

    }
}
