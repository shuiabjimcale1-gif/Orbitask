using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IColumnService
    {
        Task<Column?> GetColumn(int columnId);
        Task<IEnumerable<Column>> GetColumnsForBoard(int boardId);
        Task<Column?> CreateColumn(int boardId, Column newColumn);
        Task<Column?> UpdateColumn(int columnId, Column updated);
        Task<bool> DeleteColumn(int columnId);
    }
}