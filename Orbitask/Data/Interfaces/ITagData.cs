using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface ITagData
    {
        // Core CRUD
        Task<Tag?> GetTag(int tagId);
        Task<IEnumerable<Tag>> GetTagsForBoard(int boardId);
        Task<Tag> InsertTag(Tag tag);
        Task<Tag?> UpdateTag(Tag tag);
        Task<bool> DeleteTag(int tagId);

        // Existence checks
        Task<bool> TagExists(int tagId);
        Task<bool> BoardExists(int boardId);

        // Board lookup
        Task<int?> GetBoardIdForTag(int tagId);

        // ✅ Tenancy helper
        Task<int?> GetWorkbenchIdForTag(int tagId);
    }
}