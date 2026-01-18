using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardData boardData;
        private readonly IWorkbenchData _workbenchData;

        public BoardService(IBoardData boardData, IWorkbenchData workbenchData)
        {
            this.boardData = boardData;
            _workbenchData = workbenchData;
        }

        public async Task<Board?> GetBoard(int boardId)
        {
            return await boardData.GetBoard(boardId);
        }

        public async Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId)
        {
            return await boardData.GetBoardsForWorkbench(workbenchId);
        }

        public async Task<Board?> CreateBoard(int workbenchId, Board newBoard)
        {
            var workbench = await _workbenchData.GetWorkbench(workbenchId);
            if (workbench == null)
                return null;

            newBoard.WorkbenchId = workbenchId;

            return await boardData.InsertBoard(newBoard);
        }


        public async Task<Board?> UpdateBoard(int boardId, Board updated)
        {
            // Load existing board (ensures it exists and gives us WorkbenchId)
            var existing = await boardData.GetBoard(boardId);
            if (existing == null)
                return null;

            // Override sensitive fields
            updated.Id = boardId;
            updated.WorkbenchId = existing.WorkbenchId;

            // Update
            var success = await boardData.UpdateBoard(updated);
            return success ? updated : null;
        }


        public async Task<bool> DeleteBoard(int boardId)
        {
            if (!await boardData.BoardExists(boardId))
                return false;

            return await boardData.DeleteBoard(boardId);
        }

    }
}
