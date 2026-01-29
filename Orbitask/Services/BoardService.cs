using Orbitask.Data.Boards.Interfaces;
using Orbitask.Data.Workbenches.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardData _boardData;
        private readonly IWorkbenchData _workbenchData;

        public BoardService(IBoardData boardData, IWorkbenchData workbenchData)
        {
            _boardData = boardData;
            _workbenchData = workbenchData;
        }

        // ============================================
        // GET SINGLE BOARD
        // ============================================

        public async Task<Board?> GetBoard(int boardId)
        {
            return await _boardData.GetBoard(boardId);
        }

        // ============================================
        // GET BOARDS FOR WORKBENCH
        // ============================================

        public async Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId)
        {
            return await _boardData.GetBoardsForWorkbench(workbenchId);
        }

        // ============================================
        // CREATE BOARD
        // ============================================

        public async Task<Board?> CreateBoard(int workbenchId, Board newBoard)
        {
            // 1. Validate workbench exists
            var workbench = await _workbenchData.GetWorkbench(workbenchId);
            if (workbench == null)
                return null;

            // 2. Set workbench FK
            newBoard.WorkbenchId = workbenchId;

            // 3. Insert
            return await _boardData.InsertBoard(newBoard);
        }

        // ============================================
        // UPDATE BOARD
        // ============================================

        public async Task<Board?> UpdateBoard(int boardId, Board updated)
        {
            // 1. Load existing board (ensures it exists and gives us WorkbenchId)
            var existing = await _boardData.GetBoard(boardId);
            if (existing == null)
                return null;

            // 2. Override sensitive fields
            updated.Id = boardId;
            updated.WorkbenchId = existing.WorkbenchId;  // 🔒 Can't change workbench

            // 3. Update (now returns the updated board)
            return await _boardData.UpdateBoard(updated);
        }

        // ============================================
        // DELETE BOARD
        // ============================================

        public async Task<bool> DeleteBoard(int boardId)
        {
            // Validate board exists
            if (!await _boardData.BoardExists(boardId))
                return false;

            return await _boardData.DeleteBoard(boardId);
        }
    }
}