using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IBoardData
    {
        // Core CRUD
        Task<Board?> GetBoard(int boardId);
        Task<IEnumerable<Board>> GetBoardsForWorkbench(int workbenchId);
        Task<Board> InsertBoard(Board board);
        Task<Board?> UpdateBoard(Board board);
        Task<bool> DeleteBoard(int boardId);

        // Existence checks
        Task<bool> BoardExists(int boardId);
        Task<bool> WorkbenchExists(int workbenchId);
    }
}