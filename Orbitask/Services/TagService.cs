using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class TagService : ITagService
    {
        private readonly ITagData _tagData;
        private readonly IBoardData _boardData;

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
            var board = await _boardData.GetBoard(boardId);
            if (board == null)
                return null;

            newTag.BoardId = boardId;
            newTag.WorkbenchId = board.WorkbenchId;

            return await _tagData.InsertTag(newTag);
        }



        // UPDATE TAG

        public async Task<Tag?> UpdateTag(int tagId, Tag updated)
        {
            // Load existing tag (ensures it exists)
            var existing = await _tagData.GetTag(tagId);
            if (existing == null)
                return null;

            // Load the board (authoritative WorkbenchId)
            var board = await _boardData.GetBoard(existing.BoardId);
            if (board == null)
                return null;

            // Override sensitive fields
            updated.Id = tagId;
            updated.BoardId = existing.BoardId;
            updated.WorkbenchId = board.WorkbenchId;

            // Update
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
