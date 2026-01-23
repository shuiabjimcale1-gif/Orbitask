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
    public class TaskItemController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;
        private readonly IColumnService _columnService;
        private readonly IWorkbenchService _workbenchService;
        private readonly IConfiguration _configuration;

        public TaskItemController(
            ITaskItemService taskItemService,
            IWorkbenchService workbenchService,
            IColumnService columnService,
            IConfiguration configuration)
        {
            _taskItemService = taskItemService;
            _workbenchService = workbenchService;
            _columnService = columnService;
            _configuration = configuration;
        }

        // ============================================
        // HELPER METHODS FOR TENANCY CHECKS
        // ============================================

        /// <summary>
        /// Gets the WorkbenchId for a task by JOINing through the hierarchy.
        /// Used for authorization checks.
        /// </summary>
        private async Task<int?> GetWorkbenchIdForTask(int taskId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM TaskItems t
                INNER JOIN Columns c ON t.ColumnId = c.Id
                INNER JOIN Boards b ON c.BoardId = b.Id
                WHERE t.Id = @TaskId",
                new { TaskId = taskId }
            );
        }

        /// <summary>
        /// Gets the WorkbenchId for a column by JOINing through the hierarchy.
        /// Used for authorization checks.
        /// </summary>
        private async Task<int?> GetWorkbenchIdForColumn(int columnId)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);

            return await connection.QuerySingleOrDefaultAsync<int?>(@"
                SELECT b.WorkbenchId 
                FROM Columns c
                INNER JOIN Boards b ON c.BoardId = b.Id
                WHERE c.Id = @ColumnId",
                new { ColumnId = columnId }
            );
        }

        // ============================================
        // GET ALL TASKS FOR A COLUMN
        // ============================================

        /// <summary>
        /// GET /api/columns/{columnId}/tasks
        /// Returns all tasks in a specific column
        /// </summary>
        [HttpGet("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> GetTasksForColumn(int columnId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. 🔒 TENANCY WALL: Check column access first
            var workbenchId = await GetWorkbenchIdForColumn(columnId);
            if (workbenchId == null)
                return NotFound("Column not found");

            // 2. 🔒 TENANCY WALL: Verify user is member of workbench
            var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
            if (membership == null)
                return Forbid();  // User doesn't belong to this workbench

            // 3. Load tasks (user has access)
            var tasks = await _taskItemService.GetTasksForColumn(columnId);

            return Ok(tasks);
        }

        // ============================================
        // GET SINGLE TASK
        // ============================================

        /// <summary>
        /// GET /api/tasks/{taskId}
        /// Returns a single task by ID
        /// </summary>
        [HttpGet("tasks/{taskId:int}")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load task
            var task = await _taskItemService.GetTask(taskId);
            if (task == null)
                return NotFound("Task not found");

            // 2. 🔒 TENANCY WALL: Get WorkbenchId via JOIN
            var workbenchId = await GetWorkbenchIdForTask(taskId);
            if (workbenchId == null)
                return NotFound("Task workbench not found");

            // 3. 🔒 TENANCY WALL: Check membership
            var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
            if (membership == null)
                return Forbid();  // User doesn't belong to this workbench

            return Ok(task);
        }

        // ============================================
        // CREATE TASK
        // ============================================

        /// <summary>
        /// POST /api/columns/{columnId}/tasks
        /// Creates a new task in a column
        /// </summary>
        [HttpPost("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> CreateTask(int columnId, [FromBody] TaskItem newTask)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. 🔒 TENANCY WALL: Check column access
                var workbenchId = await GetWorkbenchIdForColumn(columnId);
                if (workbenchId == null)
                    return NotFound("Column not found");

                // 2. 🔒 TENANCY WALL: Verify user is member
                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();  // User doesn't belong to this workbench

                // 3. Create task (service handles validation)
                var task = await _taskItemService.CreateTask(columnId, newTask);
                if (task == null)
                    return NotFound("Column not found");

                return Ok(task);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================
        // UPDATE TASK
        // ============================================

        /// <summary>
        /// PUT /api/tasks/{taskId}
        /// Updates an existing task
        /// </summary>
        [HttpPut("tasks/{taskId:int}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskItem updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. 🔒 TENANCY WALL: Check access to existing task
                var workbenchId = await GetWorkbenchIdForTask(taskId);
                if (workbenchId == null)
                    return NotFound("Task not found");

                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();

                // 2. Update task (service handles cross-board prevention)
                var task = await _taskItemService.UpdateTask(taskId, updated);
                if (task == null)
                    return NotFound("Task not found");

                return Ok(task);
            }
            catch (InvalidOperationException ex)
            {
                // Service threw validation error (e.g., trying to move to different board)
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating the task" });
            }
        }

        // ============================================
        // DELETE TASK
        // ============================================

        /// <summary>
        /// DELETE /api/tasks/{taskId}
        /// Deletes a task
        /// </summary>
        [HttpDelete("tasks/{taskId:int}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 1. 🔒 TENANCY WALL: Check access
                var workbenchId = await GetWorkbenchIdForTask(taskId);
                if (workbenchId == null)
                    return NotFound("Task not found");

                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();

                // 2. Delete task
                var success = await _taskItemService.DeleteTask(taskId);
                if (!success)
                    return NotFound("Task not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the task" });
            }
        }

        // ============================================
        // ATTACH TAG TO TASK
        // ============================================

        /// <summary>
        /// POST /api/tasks/{taskId}/tags/{tagId}
        /// Attaches a tag to a task
        /// </summary>
        [HttpPost("tasks/{taskId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> AttachTag(int taskId, int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var workbenchId = await GetWorkbenchIdForTask(taskId);
                if (workbenchId == null)
                    return NotFound("Task not found");

                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();

                var success = await _taskItemService.AttachTag(taskId, tagId);
                if (!success)
                    return NotFound("Task or tag not found");

                return Ok(new { message = "Tag attached successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while attaching the tag" });
            }
        }

        // ============================================
        // REMOVE TAG FROM TASK
        // ============================================

        /// <summary>
        /// DELETE /api/tasks/{taskId}/tags/{tagId}
        /// Removes a tag from a task
        /// </summary>
        [HttpDelete("tasks/{taskId:int}/tags/{tagId:int}")]
        public async Task<IActionResult> RemoveTag(int taskId, int tagId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var workbenchId = await GetWorkbenchIdForTask(taskId);
                if (workbenchId == null)
                    return NotFound("Task not found");

                var membership = await _workbenchService.GetMembership(workbenchId.Value, userId);
                if (membership == null)
                    return Forbid();

                // 2. Remove tag
                var success = await _taskItemService.RemoveTag(taskId, tagId);
                if (!success)
                    return NotFound("Task or tag not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while removing the tag" });
            }
        }
    }
}