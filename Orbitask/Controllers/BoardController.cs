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

        // ============================================
        // GET BOARDS FOR WORKBENCH
        // ============================================

        /// <summary>
        /// GET /api/workbenches/{workbenchId}/boards
        /// Returns all boards in a workbench
        /// </summary>
        [HttpGet("workbenches/{workbenchId:int}/boards")]
        public async Task<IActionResult> GetBoards(int workbenchId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Check membership
                var membership = await _workbenchData.GetMembership(workbenchId, userId);
                if (membership == null)
                    return Forbid();

                // Load boards
                var boards = await _boardService.GetBoardsForWorkbench(workbenchId);

                return Ok(boards);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving boards" });
            }
        }

        // ============================================
        // GET SINGLE BOARD
        // ============================================

        /// <summary>
        /// GET /api/boards/{boardId}
        /// Returns a single board by ID
        /// </summary>
        [HttpGet("boards/{boardId:int}")]
        public async Task<IActionResult> GetBoard(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load board
                var board = await _boardService.GetBoard(boardId);
                if (board == null)
                    return NotFound("Board not found");

                // 2. 🔒 TENANCY WALL: Check membership
                // Board has WorkbenchId directly - no JOIN needed!
                var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
                if (membership == null)
                    return Forbid();

                return Ok(board);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving the board" });
            }
        }

        // ============================================
        // CREATE BOARD
        // ============================================

        /// <summary>
        /// POST /api/workbenches/{workbenchId}/boards
        /// Creates a new board in a workbench
        /// </summary>
        [HttpPost("workbenches/{workbenchId:int}/boards")]
        public async Task<IActionResult> CreateBoard(int workbenchId, [FromBody] Board newBoard)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchData.GetMembership(workbenchId, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // Create board (service sets WorkbenchId)
                var board = await _boardService.CreateBoard(workbenchId, newBoard);
                if (board == null)
                    return NotFound("Workbench not found");

                return Ok(board);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the board" });
            }
        }

        // ============================================
        // UPDATE BOARD
        // ============================================

        /// <summary>
        /// PUT /api/boards/{boardId}
        /// Updates an existing board
        /// </summary>
        [HttpPut("boards/{boardId:int}")]
        public async Task<IActionResult> UpdateBoard(int boardId, [FromBody] Board updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load existing board
                var board = await _boardService.GetBoard(boardId);
                if (board == null)
                    return NotFound("Board not found");

                // 2. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 3. Update (service prevents WorkbenchId change)
                var updatedBoard = await _boardService.UpdateBoard(boardId, updated);
                if (updatedBoard == null)
                    return NotFound("Board not found");

                return Ok(updatedBoard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the board" });
            }
        }

        // ============================================
        // DELETE BOARD
        // ============================================

        /// <summary>
        /// DELETE /api/boards/{boardId}
        /// Deletes a board and all its contents
        /// </summary>
        [HttpDelete("boards/{boardId:int}")]
        public async Task<IActionResult> DeleteBoard(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load board
                var board = await _boardService.GetBoard(boardId);
                if (board == null)
                    return NotFound("Board not found");

                // 2. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchData.GetMembership(board.WorkbenchId, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 3. Delete
                var success = await _boardService.DeleteBoard(boardId);
                if (!success)
                    return NotFound("Board not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the board" });
            }
        }
    }
}