
using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IBoardService
    {
        Task<Board?> CreateBoard(int workbenchId, Board newBoard);
        Task<bool> DeleteBoard(int boardId);
        Task<Board?> GetBoard(int boardId);
        Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId);
        Task<Board?> UpdateBoard(int boardId, Board updated);
    }
}
