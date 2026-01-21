using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services;
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

        public TagController(ITagService _tagService, IWorkbenchService workbenchService, IBoardService boardService)
        {
            this._tagService = _tagService;
            this._workbenchService = workbenchService;
            this._boardService = boardService;
        }

        [HttpPost("boards/{boardId:int}/tags")]
        public async Task<IActionResult> CreateTag(int boardId, [FromBody] Tag newTag)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the board from DB (never trust client WorkbenchId)
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(board.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 3. Create the tag (service will assign WorkbenchId from board)
            var tag = await _tagService.CreateTag(boardId, newTag);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }


        [HttpGet("tags/{tagId:int}")]
        public async Task<IActionResult> GetTag(int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the tag from DB (trusted source)
            var tag = await _tagService.GetTag(tagId);
            if (tag == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(tag.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Return the tag
            return Ok(tag);
        }


        [HttpPut("tags/{tagId:int}")]
        public async Task<IActionResult> UpdateTag(int tagId, [FromBody] Tag updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the tag from DB (never trust client IDs)
            var existing = await _tagService.GetTag(tagId);
            if (existing == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(existing.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 3. Perform the update
            var tag = await _tagService.UpdateTag(tagId, updated);
            if (tag == null)
                return NotFound();

            return Ok(tag);
        }


        [HttpDelete("tags/{tagId:int}")]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the tag from DB (never trust client IDs)
            var tag = await _tagService.GetTag(tagId);
            if (tag == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(tag.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 3. Delete the tag
            var success = await _tagService.DeleteTag(tagId);
            if (!success)
                return NotFound();

            return NoContent();
        }

    }
}
