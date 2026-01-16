using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardData boardData;

        public BoardService(IBoardData boardData)
        {
            this.boardData = boardData;

        }

        public async Task<Board?> GetBoard(int boardId)
        {
            return await boardData.GetBoard(boardId);
        }

        public async Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId)
        {
            return await boardData.GetBoardsForWorkbench(workbenchId);
        }

        public async Task<Board?> CreateBoard(int WorkbenchId, Board newBoard)
        {
            if (!await boardData.WorkbenchExists(WorkbenchId))
                return null;

            newBoard.WorkbenchId = WorkbenchId;

            await boardData.InsertBoard(newBoard);
            return newBoard;
        }

        public async Task<Board?> UpdateBoard(int boardId, Board updated)
        {
            if (!await boardData.BoardExists(boardId))
                return null;

            updated.Id = boardId;

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
