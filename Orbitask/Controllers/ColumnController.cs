using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [ApiController]

    [Authorize]
    [Route("api")]
    public class ColumnController : ControllerBase
    {
        private readonly IColumnService _columnService;
        private readonly IBoardService _boardService;
        private readonly IWorkbenchService _workbenchService;

        public ColumnController(
            IColumnService columnService, 
            IBoardService boardService, 
            IWorkbenchService workbenchService)
        {
            _columnService = columnService;
            _boardService = boardService;
            _workbenchService = workbenchService;
        }

        [HttpGet("boards/{boardId:int}/columns")]
        public async Task<IActionResult> GetColumns(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the board from DB (never trust client IDs)
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(board.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Load columns
            var columns = await _columnService.GetColumnsForBoard(boardId);

            return Ok(columns);
        }


        [HttpGet("columns/{columnId:int}")]
        public async Task<IActionResult> GetColumn(int columnId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the column from DB (trusted source)
            var column = await _columnService.GetColumn(columnId);
            if (column == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(column.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Return the column
            return Ok(column);
        }



        [HttpPost("boards/{boardId:int}/columns")]
        public async Task<IActionResult> CreateColumn(int boardId, [FromBody] Column newColumn)
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

            // 3. Create the column (service will assign WorkbenchId)
            var column = await _columnService.CreateColumn(boardId, newColumn);
            if (column == null)
                return NotFound();

            return Ok(column);
        }


        [HttpPut("columns/{columnId:int}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] Column updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the column from DB (never trust client IDs)
            var column = await _columnService.GetColumn(columnId);
            if (column == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId from DB
            var membership = await _workbenchService.GetMembership(column.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 3. Perform the update
            var updatedColumn = await _columnService.UpdateColumn(columnId, updated);
            if (updatedColumn == null)
                return NotFound();

            return Ok(updatedColumn);
        }



        [HttpDelete("columns/{columnId:int}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the column from DB (never trust client IDs)
            var column = await _columnService.GetColumn(columnId);
            if (column == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId from DB
            var membership = await _workbenchService.GetMembership(column.WorkbenchId, userId);
            if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 3. Perform delete
            var success = await _columnService.DeleteColumn(columnId);
            if (!success)
                return NotFound();
            
            return NoContent();
        }

    }
}

