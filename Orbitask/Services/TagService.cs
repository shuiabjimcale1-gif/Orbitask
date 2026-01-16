using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class TagService : ITagService
    {
        private readonly ITagData _tagData;

        public TagService(ITagData tagData)
        {
            _tagData = tagData;
        }


        // GET TAG

        public async Task<Tag?> GetTag(int tagId)
        {
            return await _tagData.GetTag(tagId);
        }


        // CREATE TAG

        public async Task<Tag?> CreateTag(int boardId, Tag newTag)
        {
            // Validate board exists
            if (!await _tagData.BoardExists(boardId))
                return null;

            newTag.BoardId = boardId;

            var createdTag =await _tagData.InsertTag(newTag);
            return createdTag;
        }


        // UPDATE TAG

        public async Task<Tag?> UpdateTag(int tagId, Tag updated)
        {
            // Validate tag exists
            if (!await _tagData.TagExists(tagId))
                return null;

            // Keep ID consistent
            updated.Id = tagId;

            // Ensure board is still valid
            var boardId = await _tagData.GetBoardIdForTag(tagId);
            if (boardId == null)
                return null;

            updated.BoardId = boardId.Value;

            var success = await _tagData.UpdateTag(updated);
            return success ? updated : null;
        }


        // DELETE TAG

        public async Task<bool> DeleteTag(int tagId)
        {
            // Validate tag exists
            if (!await _tagData.TagExists(tagId))
                return false;

            return await _tagData.DeleteTag(tagId);
        }
    }
}
