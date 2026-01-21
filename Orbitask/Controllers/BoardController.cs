
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api")]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly IWorkbenchData _workbenchData;

        public BoardController(IBoardService boardService, IWorkbenchData workbenchData)
        {
            _boardService = boardService;
            _workbenchData = workbenchData;
        }

        [HttpGet("workbenches/{workspaceId:int}/boards")]
        public async Task<IActionResult> GetBoards(int workspaceId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var membership = await _workbenchData.GetMembership(workspaceId, userId);
            if (membership == null)
                return Forbid();

            var boards = await _boardService.GetBoardsForWorkbench(workspaceId);
            return Ok(boards);
        }

        [HttpGet("boards/{boardId:int}")]
        public async Task<IActionResult> GetBoard(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            return Ok(board);
        }

        [HttpPost("workbenches/{workspaceId:int}/boards")]
        public async Task<IActionResult> CreateBoard(int workspaceId, [FromBody] Board newBoard)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var membership = await _workbenchData.GetMembership(workspaceId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            var board = await _boardService.CreateBoard(workspaceId, newBoard);
            if (board == null)
                return NotFound();

            return Ok(board);
        }

        [HttpPut("boards/{boardId:int}")]
        public async Task<IActionResult> UpdateBoard(int boardId, [FromBody] Board updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();
            var updatedBoard = await _boardService.UpdateBoard(boardId, updated);
            if (updatedBoard == null) 
                return NotFound();

            return Ok(updatedBoard);
        }

        [HttpDelete("boards/{boardId:int}")]
        public async Task<IActionResult> DeleteBoard(int boardId)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            var success = await _boardService.DeleteBoard(boardId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
