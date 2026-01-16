using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IColumnData
    {
        Task<bool> BoardExists(int boardId);
        Task<bool> ColumnExists(int columnId);
        Task<bool> DeleteColumn(int columnId);
        Task<int?> GetBoardIdForColumn(int columnId);
        Task<Column?> GetColumn(int columnId);
        Task<IEnumerable<Column>> GetColumnsForBoard(int boardId);
        Task<Column?> InsertColumn(Column newColumn);
        Task<Column?> UpdateColumn(Column updated);
    }
}
