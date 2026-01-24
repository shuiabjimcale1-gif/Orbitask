using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface ITagService
    {
        Task<Tag?> GetTag(int tagId);
        Task<IEnumerable<Tag>> GetTagsForBoard(int boardId);
        Task<Tag?> CreateTag(int boardId, Tag newTag);
        Task<Tag?> UpdateTag(int tagId, Tag updated);
        Task<bool> DeleteTag(int tagId);
    }
}