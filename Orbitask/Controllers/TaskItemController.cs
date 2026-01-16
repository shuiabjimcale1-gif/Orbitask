using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Controllers
{
    [Route("api")]
    [ApiController]
    public class TaskItemController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;

        public TaskItemController(ITaskItemService _taskItemService)
        {
            this._taskItemService = _taskItemService;
        }

        [HttpGet]
        [Route("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> GetTasksForColumn(int columnId)
        {
            var tasks = await _taskItemService.GetTasksForColumn(columnId);
            return Ok(tasks);
        }



        [HttpPost]
        [Route("columns/{columnId:int}/tasks")]
        public async Task<IActionResult> CreateTask(int columnId, [FromBody] TaskItem newTaskItem)
        {
            var task = await _taskItemService.CreateTask(columnId, newTaskItem);
            if (task == null)
                return NotFound();
            return Ok(task);
        }


        [HttpGet]
        [Route("tasks/{taskId:int}")]
        public async Task<IActionResult> GetTask(int taskId)
        {
            var task = await _taskItemService.GetTask(taskId);

            if (task == null)
                return NotFound();

            return Ok(task);
        }


        [HttpPut]
        [Route("tasks/{taskId:int}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskItem updated)
        {
            var task = await _taskItemService.UpdateTask(taskId, updated);

            if (task == null)
                return NotFound();

            return Ok(task);
        }




        [HttpDelete]
        [Route("tasks/{taskId:int}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var success = await _taskItemService.DeleteTask(taskId);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
