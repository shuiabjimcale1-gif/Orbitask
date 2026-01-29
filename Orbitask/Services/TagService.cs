using Orbitask.Data.Boards.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class TagService : ITagService
    {
        private readonly ITagData _tagData;
        private readonly IBoardData _boardData;

        // ✅ FIXED: Added IBoardData to constructor
        public TagService(ITagData tagData, IBoardData boardData)
        {
            _tagData = tagData;
            _boardData = boardData;
        }

        // ============================================
        // GET SINGLE TAG
        // ============================================

        public async Task<Tag?> GetTag(int tagId)
        {
            return await _tagData.GetTag(tagId);
        }

        // ============================================
        // GET TAGS FOR BOARD
        // ============================================

        public async Task<IEnumerable<Tag>> GetTagsForBoard(int boardId)
        {
            return await _tagData.GetTagsForBoard(boardId);
        }

        // ============================================
        // CREATE TAG
        // ============================================

        public async Task<Tag?> CreateTag(int boardId, Tag newTag)
        {
            // 1. Validate board exists
            var board = await _boardData.GetBoard(boardId);
            if (board == null)
                return null;

            // 2. Set only direct parent FK
            newTag.BoardId = boardId;


            // 3. Insert
            return await _tagData.InsertTag(newTag);
        }

        // ============================================
        // UPDATE TAG
        // ============================================

        public async Task<Tag?> UpdateTag(int tagId, Tag updated)
        {
            // 1. Load existing tag (ensures it exists)
            var existing = await _tagData.GetTag(tagId);
            if (existing == null)
                return null;

            updated.Id = tagId;
            updated.BoardId = existing.BoardId;  

            // 3. Update (now returns the updated tag)
            return await _tagData.UpdateTag(updated);
        }

        // ============================================
        // DELETE TAG
        // ============================================

        public async Task<bool> DeleteTag(int tagId)
        {
            // Validate tag exists
            if (!await _tagData.TagExists(tagId))
                return false;

            return await _tagData.DeleteTag(tagId);
        }
    }
}