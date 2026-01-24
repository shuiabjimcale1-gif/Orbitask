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

        // ============================================
        // GET SINGLE COLUMN
        // ============================================

        public async Task<Column?> GetColumn(int columnId)
        {
            return await _columnData.GetColumn(columnId);
        }

        // ============================================
        // GET COLUMNS FOR BOARD
        // ============================================

        public async Task<IEnumerable<Column>> GetColumnsForBoard(int boardId)
        {
            return await _columnData.GetColumnsForBoard(boardId);
        }

        // ============================================
        // CREATE COLUMN
        // ============================================

        public async Task<Column?> CreateColumn(int boardId, Column newColumn)
        { 
            var board = await _boardData.GetBoard(boardId);
            if (board == null)
                return null;
            newColumn.BoardId = boardId;
            return await _columnData.InsertColumn(newColumn);
        }

        // ============================================
        // UPDATE COLUMN
        // ============================================

        public async Task<Column?> UpdateColumn(int columnId, Column updated)
        {
            var existing = await _columnData.GetColumn(columnId);
            if (existing == null)
                return null;

            updated.Id = columnId;
            updated.BoardId = existing.BoardId; 

            return await _columnData.UpdateColumn(updated);
        }

        // ============================================
        // DELETE COLUMN
        // ============================================

        public async Task<bool> DeleteColumn(int columnId)
        {
            // Validate exists
            if (!await _columnData.ColumnExists(columnId))
                return false;

            return await _columnData.DeleteColumn(columnId);
        }
    }
}