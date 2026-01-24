using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IColumnData
    {
        // Core CRUD
        Task<Column?> GetColumn(int columnId);
        Task<IEnumerable<Column>> GetColumnsForBoard(int boardId);
        Task<Column?> InsertColumn(Column newColumn);
        Task<Column?> UpdateColumn(Column updated);
        Task<bool> DeleteColumn(int columnId);

        // Existence checks
        Task<bool> ColumnExists(int columnId);
        Task<bool> BoardExists(int boardId);

        // Helpers
        Task<int?> GetBoardIdForColumn(int columnId);

        // ✅ Tenancy helper
        Task<int?> GetWorkbenchIdForColumn(int columnId);
    }
}