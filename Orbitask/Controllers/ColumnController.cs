using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using Orbitask.Models;
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
        private readonly IConfiguration _configuration;

        public ColumnController(
            IColumnService columnService,
            IBoardService boardService,
            IWorkbenchService workbenchService,
            IConfiguration configuration)
        {
            _columnService = columnService;
            _boardService = boardService;
            _workbenchService = workbenchService;
            _configuration = configuration;
        }

        // ============================================
        // HELPER METHOD - TENANCY CHECK
        // ============================================

        /// <summary>
        /// Gets WorkbenchId for a column by going up the tree
        /// </summary>
        private async Task<int?> GetWorkbenchIdForColumn(int columnId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            // Go up tree: Column → Board → Workbench
            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM Columns c
                INNER JOIN Boards b ON c.BoardId = b.Id
                WHERE c.Id = @ColumnId",
                new { ColumnId = columnId }
            );
        }

        // ============================================
        // GET COLUMNS FOR BOARD
        // ============================================

        /// <summary>
        /// GET /api/boards/{boardId}/columns
        /// Returns all columns in a board
        /// </summary>
        [HttpGet("boards/{boardId:int}/columns")]
        public async Task<IActionResult> GetColumns(int boardId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load board (to verify it exists)
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound("Board not found");

            // 2. 🔒 TENANCY WALL: Check membership
            var membership = await _workbenchService.GetMembership(board.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Load columns
            var columns = await _columnService.GetColumnsForBoard(boardId);

            return Ok(columns);
        }

        // ============================================
        // GET SINGLE COLUMN
        // ============================================

        /// <summary>
        /// GET /api/columns/{columnId}
        /// Returns a single column by ID
        /// </summary>
        [HttpGet("columns/{columnId:int}")]
        public async Task<IActionResult> GetColumn(int columnId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load column
            var column = await _columnService.GetColumn(columnId);
            if (column == null)
                return NotFound("Column not found");

            // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
            var workbenchId = await GetWorkbenchIdForColumn(columnId);
            if (workbenchId == null)
                return NotFound("Column workbench not found");

            // 3. 🔒 TENANCY WALL: Check membership
            var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
            if (membership == null)
                return Forbid();

            return Ok(column);
        }

        // ============================================
        // CREATE COLUMN
        // ============================================

        /// <summary>
        /// POST /api/boards/{boardId}/columns
        /// Creates a new column in a board
        /// </summary>
        [HttpPost("boards/{boardId:int}/columns")]
        public async Task<IActionResult> CreateColumn(int boardId, [FromBody] Column newColumn)
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

                // 3. Create column (service handles setting BoardId)
                var column = await _columnService.CreateColumn(boardId, newColumn);
                if (column == null)
                    return NotFound("Board not found");

                return Ok(column);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while creating the column" });
            }
        }

        // ============================================
        // UPDATE COLUMN
        // ============================================

        /// <summary>
        /// PUT /api/columns/{columnId}
        /// Updates an existing column
        /// </summary>
        [HttpPut("columns/{columnId:int}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] Column updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. Load existing column
                var column = await _columnService.GetColumn(columnId);
                if (column == null)
                    return NotFound("Column not found");

                // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
                var workbenchId = await GetWorkbenchIdForColumn(columnId);
                if (workbenchId == null)
                    return NotFound("Column workbench not found");

                // 3. 🔒 TENANCY WALL: Check membership with ADMIN role
                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                // 4. Update (service handles validation)
                var updatedColumn = await _columnService.UpdateColumn(columnId, updated);
                if (updatedColumn == null)
                    return NotFound("Column not found");

                return Ok(updatedColumn);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the column" });
            }
        }

        // ============================================
        // DELETE COLUMN
        // ============================================

        /// <summary>
        /// DELETE /api/columns/{columnId}
        /// Deletes a column and all its tasks
        /// </summary>
        [HttpDelete("columns/{columnId:int}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var column = await _columnService.GetColumn(columnId);
                if (column == null)
                    return NotFound("Column not found");

                var workbenchId = await GetWorkbenchIdForColumn(columnId);
                if (workbenchId == null)
                    return NotFound("Column workbench not found");

                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null || membership.Role != WorkbenchMember.WorkbenchRole.Admin)
                    return Forbid();

                var success = await _columnService.DeleteColumn(columnId);
                if (!success)
                    return NotFound("Column not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the column" });
            }
        }
    }
}