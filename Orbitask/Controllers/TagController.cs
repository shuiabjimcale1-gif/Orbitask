using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using Orbitask.Models;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [Route("api")]
    [Authorize]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IWorkbenchService _workbenchService;
        private readonly IBoardService _boardService;
        private readonly IConfiguration _configuration;

        public TagController(
            ITagService tagService,
            IWorkbenchService workbenchService,
            IBoardService boardService,
            IConfiguration configuration)
        {
            _tagService = tagService;
            _workbenchService = workbenchService;
            _boardService = boardService;
            _configuration = configuration;
        }

        // ============================================
        // HELPER METHOD - TENANCY CHECK
        // ============================================

        /// <summary>
        /// Gets WorkbenchId for a tag by going up the tree
        /// </summary>
        private async Task<int?> GetWorkbenchIdForTag(int tagId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            // Go up tree: Tag → Board → Workbench
            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM Tags t
                INNER JOIN Boards b ON t.BoardId = b.Id
                WHERE t.Id = @TagId",
                new { TagId = tagId }
            );
        }

        // ============================================
        // GET TAGS FOR BOARD
        // ============================================

        /// <summary>
        /// GET /api/boards/{boardId}/tags
        /// Returns all tags for a board
        /// </summary>
        [HttpGet("boards/{boardId:int}/tags")]
        public async Task<IActionResult> GetTagsForBoard(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load board (to verify it exists)
                var board = await _boardService.GetBoard(boardId);
                if (board == null)
                    return NotFound("Board not found");

                // 2. 🔒 TENANCY WALL: Check membership
                var membership = await _workbenchService.GetMembership(board.WorkbenchId, userId);
                if (membership == null)
                    return Forbid();

                // 3. Load tags
                var tags = await _tagService.GetTagsForBoard(boardId);

                return Ok(tags);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving tags" });
            }
        }

        // ============================================
        // GET SINGLE TAG
        // ============================================

        /// <summary>
        /// GET /api/tags/{tagId}
        /// Returns a single tag by ID
        /// </summary>
        [HttpGet("tags/{tagId:int}")]
        public async Task<IActionResult> GetTag(int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load tag
                var tag = await _tagService.GetTag(tagId);
                if (tag == null)
                    return NotFound("Tag not found");

                // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
                var workbenchId = await GetWorkbenchIdForTag(tagId);
                if (workbenchId == null)
                    return NotFound("Tag workbench not found");

                // 3. 🔒 TENANCY WALL: Check membership
                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the tag" });
            }
        }

        // ============================================
        // CREATE TAG
        // ============================================

        /// <summary>
        /// POST /api/boards/{boardId}/tags
        /// Creates a new tag for a board
        /// </summary>
        [HttpPost("boards/{boardId:int}/tags")]
        public async Task<IActionResult> CreateTag(int boardId, [FromBody] Tag newTag)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load board (to verify it exists and get workbenchId)
                var board = await _boardService.GetBoard(boardId);
                if (board == null)
                    return NotFound("Board not found");

                // 2. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchService.GetMembership(board.WorkbenchId, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 3. Create tag (service sets BoardId)
                var tag = await _tagService.CreateTag(boardId, newTag);
                if (tag == null)
                    return NotFound("Board not found");

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the tag" });
            }
        }

        // ============================================
        // UPDATE TAG
        // ============================================

        /// <summary>
        /// PUT /api/tags/{tagId}
        /// Updates an existing tag
        /// </summary>
        [HttpPut("tags/{tagId:int}")]
        public async Task<IActionResult> UpdateTag(int tagId, [FromBody] Tag updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load existing tag
                var existing = await _tagService.GetTag(tagId);
                if (existing == null)
                    return NotFound("Tag not found");

                // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
                var workbenchId = await GetWorkbenchIdForTag(tagId);
                if (workbenchId == null)
                    return NotFound("Tag workbench not found");

                // 3. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 4. Update (service prevents BoardId change)
                var tag = await _tagService.UpdateTag(tagId, updated);
                if (tag == null)
                    return NotFound("Tag not found");

                return Ok(tag);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the tag" });
            }
        }

        // ============================================
        // DELETE TAG
        // ============================================

        /// <summary>
        /// DELETE /api/tags/{tagId}
        /// Deletes a tag and removes it from all tasks
        /// </summary>
        [HttpDelete("tags/{tagId:int}")]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load tag
                var tag = await _tagService.GetTag(tagId);
                if (tag == null)
                    return NotFound("Tag not found");

                // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
                var workbenchId = await GetWorkbenchIdForTag(tagId);
                if (workbenchId == null)
                    return NotFound("Tag workbench not found");

                // 3. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 4. Delete (removes from all tasks first)
                var success = await _tagService.DeleteTag(tagId);
                if (!success)
                    return NotFound("Tag not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the tag" });
            }
        }
    }
}