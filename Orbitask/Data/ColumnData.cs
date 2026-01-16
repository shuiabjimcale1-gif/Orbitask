using Dapper;
using Microsoft.Data.SqlClient;
using Orbitask.Data.Interfaces;
using Orbitask.Models;

namespace Orbitask.Data
{
    public class ColumnData : IColumnData
    {
        private readonly string _connectionString;

        public ColumnData(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public async Task<Column?> GetColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<Column>(
                "SELECT * FROM Columns WHERE Id = @Id",
                new { Id = columnId }
            );
        }

        public async Task<IEnumerable<Column>> GetColumnsForBoard(int boardId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryAsync<Column>(
                "SELECT * FROM Columns WHERE BoardId = @BoardId ORDER BY Position",
                new { BoardId = boardId }
            );
        }

        public async Task<Column?> InsertColumn(Column column)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"INSERT INTO Columns (BoardId, Title, Position)
                        OUTPUT INSERTED.Id,
                            INSERTED.BoardId,
                            INSERTED.Title,
                            INSERTED.Position
                        VALUES (@BoardId, @Title, @Position);
                        ";

            var columnCreated = await connection.QuerySingleAsync<Column>(sql, column);
            return columnCreated;
        }

        public async Task<Column?> UpdateColumn(Column column)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                        UPDATE Columns
                        SET Title = @Title,
                            Position = @Position
                        OUTPUT
                            INSERTED.Id,
                            INSERTED.BoardId,
                            INSERTED.Title,
                            INSERTED.Position
                        WHERE Id = @Id;
                        ";

            return await connection.QuerySingleOrDefaultAsync<Column>(sql, column);
        }


        public async Task<bool> DeleteColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var tx = connection.BeginTransaction();

            try
            {
                // 1) Delete TaskTags for tasks in this column
                await connection.ExecuteAsync(
                    "DELETE TT FROM TaskTags TT INNER JOIN TaskItems TI ON TI.Id = TT.TaskItemId WHERE TI.ColumnId = @ColumnId;",
                    new { ColumnId = columnId },
                    tx
                );

                // 2) Delete TaskItems in this column
                await connection.ExecuteAsync(
                    "DELETE FROM TaskItems WHERE ColumnId = @ColumnId;",
                    new { ColumnId = columnId },
                    tx
                );

                // 3) Delete the Column
                var rows = await connection.ExecuteAsync(
                    "DELETE FROM Columns WHERE Id = @Id;",
                    new { Id = columnId },
                    tx
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


        public async Task<bool> ColumnExists(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.ExecuteScalarAsync<bool>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM Columns WHERE Id = @Id) THEN 1 ELSE 0 END",
                new { Id = columnId }
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

        public async Task<int?> GetBoardIdForColumn(int columnId)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QuerySingleOrDefaultAsync<int?>(
                "SELECT BoardId FROM Columns WHERE Id = @Id;",
                new { Id = columnId }
            );
        }
    }
}

