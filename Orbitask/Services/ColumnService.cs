using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class ColumnService : IColumnService
    {
        private readonly IColumnData _columnData;

        public ColumnService(IColumnData columnData)
        {
            _columnData = columnData;
        }

        public async Task<Column?> GetColumn(int columnId)
        {
            return await _columnData.GetColumn(columnId);
        }

        public async Task<IEnumerable<Column>> GetColumnsForBoard(int boardId)
        {
            return await _columnData.GetColumnsForBoard(boardId);
        }

        public async Task<Column?> CreateColumn(int boardId, Column newColumn)
        {
            if (!await _columnData.BoardExists(boardId))
                return null;

            newColumn.BoardId = boardId;

            var createdColumn = await _columnData.InsertColumn(newColumn);
            return createdColumn;
        }

        public async Task<Column?> UpdateColumn(int columnId, Column updated)
        {
            updated.Id = columnId;

            return await _columnData.UpdateColumn(updated);
        }


        public async Task<bool> DeleteColumn(int columnId)
        {
            return await _columnData.DeleteColumn(columnId);
        }
    }
}
