using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IBoardData
    {
        Task<Board?> GetBoard(int boardId);
        Task<IEnumerable<Board>> GetBoardsForWorkbench(int workspaceId);

        Task<Board> InsertBoard(Board board);
        Task<bool> UpdateBoard(Board board);
        Task<bool> DeleteBoard(int boardId);

        // Existence checks
        Task<bool> BoardExists(int boardId);
        Task<bool> WorkbenchExists(int workspaceId);




    }
}
