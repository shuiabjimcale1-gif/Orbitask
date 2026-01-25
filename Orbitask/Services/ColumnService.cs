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
            // 1. Validate board exists
            var board = await _boardData.GetBoard(boardId);
            if (board == null)
                return null;

            // 2. Set only direct parent FK
            newColumn.BoardId = boardId;

            // ❌ REMOVED: newColumn.WorkbenchId = board.WorkbenchId;
            // WorkbenchId now derived via JOIN when needed

            // 3. Insert
            return await _columnData.InsertColumn(newColumn);
        }

        // ============================================
        // UPDATE COLUMN
        // ============================================

        public async Task<Column?> UpdateColumn(int columnId, Column updated)
        {
            // 1. Load existing column (ensures it exists)
            var existing = await _columnData.GetColumn(columnId);
            if (existing == null)
                return null;

            // 2. Apply required IDs
            updated.Id = columnId;
            updated.BoardId = existing.BoardId;  // Can't change board

            // ❌ REMOVED: updated.WorkbenchId = ...

            // 3. Update (now returns the updated column)
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