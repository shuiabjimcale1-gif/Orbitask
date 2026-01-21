using Microsoft.AspNetCore.Authorization;
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
    public class TaskItemController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;
        private readonly IColumnService _columnService;
        private readonly IWorkbenchService _workbenchService;

        public TaskItemController(ITaskItemService _taskItemService, IWorkbenchService workbenchService, IColumnService columnService)
        {
            this._taskItemService = _taskItemService;
            _workbenchService = workbenchService;
            _columnService = columnService;
        }

        [HttpGet("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> GetTasksForColumn(int columnId)
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

            // 3. Load tasks
            var tasks = await _taskItemService.GetTasksForColumn(columnId);

            return Ok(tasks);
        }




        [HttpPost("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> CreateTask(int columnId, [FromBody] TaskItem newTaskItem)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the column from DB
            var column = await _columnService.GetColumn(columnId);
            if (column == null)
                return NotFound();

            // 2. Check membership (member is enough)
            var membership = await _workbenchService.GetMembership(column.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Create the task
            var task = await _taskItemService.CreateTask(columnId, newTaskItem);
            if (task == null)
                return NotFound();

            return Ok(task);
        }



        [HttpGet("tasks/{taskId:int}")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the task from DB (trusted source)
            var task = await _taskItemService.GetTask(taskId);
            if (task == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(task.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Return the task
            return Ok(task);
        }



        [HttpPut("tasks/{taskId:int}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskItem updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the task from DB (trusted source)
            var existing = await _taskItemService.GetTask(taskId);
            if (existing == null)
                return NotFound();

            // 2. Check membership using the REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(existing.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Perform the update
            var task = await _taskItemService.UpdateTask(taskId, updated);
            if (task == null)
                return NotFound();

            return Ok(task);
        }





        [HttpDelete("tasks/{taskId:int}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 1. Load the task from DB
            var task = await _taskItemService.GetTask(taskId);
            if (task == null)
                return NotFound();

            // 2. Check membership using REAL WorkbenchId
            var membership = await _workbenchService.GetMembership(task.WorkbenchId, userId);
            if (membership == null)
                return Forbid();

            // 3. Delete
            var success = await _taskItemService.DeleteTask(taskId);
            if (!success)
                return NotFound();

            return NoContent();
        }

    }
}
