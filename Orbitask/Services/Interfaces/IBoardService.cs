using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IBoardService
    {
        Task<Board?> GetBoard(int boardId);
        Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId);
        Task<Board?> CreateBoard(int workbenchId, Board newBoard);
        Task<Board?> UpdateBoard(int boardId, Board updated);
        Task<bool> DeleteBoard(int boardId);
    }
}