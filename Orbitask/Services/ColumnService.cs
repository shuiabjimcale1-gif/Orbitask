using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class ColumnService : IColumnService
    {
        private readonly IColumnData _columnData;
        private readonly IBoardData _boardData;

        public ColumnService(IColumnData columnData, IBoardData boardData)
        {
            _columnData = columnData;
            _boardData = boardData;
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
            // 1. Load the board (we need its WorkbenchId)
            var board = await _boardData.GetBoard(boardId);
            if (board == null)
                return null;

            // 2. Assign both BoardId and WorkbenchId
            newColumn.BoardId = boardId;
            newColumn.WorkbenchId = board.WorkbenchId;

            // 3. Insert
            var createdColumn = await _columnData.InsertColumn(newColumn);
            return createdColumn;
        }


        public async Task<Column?> UpdateColumn(int columnId, Column updated)
        {
            // Load existing column (ensures it exists)
            var existing = await _columnData.GetColumn(columnId);
            if (existing == null)
                return null;

            // Load the board (we need WorkbenchId)
            var board = await _boardData.GetBoard(existing.BoardId);
            if (board == null)
                return null;

            // Apply required IDs
            updated.Id = columnId;
            updated.BoardId = existing.BoardId;
            updated.WorkbenchId = board.WorkbenchId;

            // Update
            var updatedColumn = await _columnData.UpdateColumn(updated);
            return updatedColumn;
        }



        public async Task<bool> DeleteColumn(int columnId)
        {
            return await _columnData.DeleteColumn(columnId);
        }
    }
}
