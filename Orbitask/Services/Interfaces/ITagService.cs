using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface ITagService
    {
        Task<Tag?> CreateTag(int boardId, Tag newTag);
        Task<bool> DeleteTag(int tagId);
        Task<Tag?> GetTag(int tagId);
        Task<Tag?> UpdateTag(int tagId, Tag updated);
    }
}