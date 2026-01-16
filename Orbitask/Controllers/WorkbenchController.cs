using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Controllers
{
    [ApiController]
    [Route("api/workbenches")]
    public class WorkbenchController : ControllerBase
    {
        private readonly IWorkbenchService _service;

        public WorkbenchController(IWorkbenchService service)
        {
            _service = service;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetWorkbench(int id)
        {
            var wb = await _service.GetWorkbench(id);
            return wb == null ? NotFound() : Ok(wb);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetWorkbenchesForUser(string userId)
        {
            var list = await _service.GetWorkbenchesForUser(userId);
            return Ok(list);
        }

        [HttpPost("user/{userId}")]
        public async Task<IActionResult> CreateWorkbench(string userId, [FromBody] Workbench wb)
        {
            var created = await _service.CreateWorkbench(userId, wb);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWorkbench(int id, [FromBody] Workbench wb)
        {
            var updated = await _service.UpdateWorkbench(id, wb);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}/user/{userId}")]
        public async Task<IActionResult> DeleteWorkbench(int id, string userId)
        {
            var success = await _service.DeleteWorkbench(id, userId);
            return success ? NoContent() : NotFound();
        }
    }

}
