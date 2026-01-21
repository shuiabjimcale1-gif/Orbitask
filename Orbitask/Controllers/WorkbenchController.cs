using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Orbitask.Models;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [ApiController]
    [Authorize]
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (await _service.GetMembership(id, userId) == null)
            {
                return Forbid();
            }
            var wb = await _service.GetWorkbench(id);
            return wb == null ? NotFound() : Ok(wb);

        }

        [HttpGet("mine")]
        public async Task<IActionResult> GetWorkbenchesForUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var list = await _service.GetWorkbenchesForUser(userId);
            return Ok(list);
        }

        [HttpPost()]
        public async Task<IActionResult> CreateWorkbench([FromBody] Workbench wb)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var created = await _service.CreateWorkbench(userId, wb);
            return Ok(created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateWorkbench(int id, [FromBody] Workbench wb)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var memberShip = await _service.GetMembership(id, userId);
            if (memberShip == null || memberShip.Role != WorkbenchMember.WorkbenchRole.Admin )
            {
                return Forbid();
            }
            var updated = await _service.UpdateWorkbench(id, wb);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteWorkbench(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wb = await _service.GetWorkbench(id);
            if (wb == null)
                return NotFound();

            if (wb.OwnerId != userId)
                return Forbid("Only the owner can delete the workbench.");
            var success = await _service.DeleteWorkbench(id, userId);
            return success ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}/users")]
        public async Task<IActionResult> GetUsersForWorkbench(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var memberShip = await _service.GetMembership(id, userId);
            if (memberShip == null || memberShip.Role != WorkbenchMember.WorkbenchRole.Admin)
            {
                return Forbid();
            }
            var users = await _service.GetUsersForWorkbench(id);

            if (users == null)
            {
                return NotFound();
            }

            return Ok(users);
        }

        [HttpPost("{id:int}/users/{userId}/{role}")]
        public async Task<IActionResult> AddUserToWorkbench(int id, string userId, WorkbenchMember.WorkbenchRole role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. Check admin
            var memberShip = await _service.GetMembership(id, currentUserId);
            if (memberShip == null || memberShip.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            // 2. Check duplicate
            var exists = await _service.GetMembership(id, userId);
            if (exists != null)
                return BadRequest("User already exists");

            // 3. Add user
            var added = await _service.AddUserToWorkbench(id, userId, role);
            return added ? Ok() : NotFound();
        }



        [HttpDelete("{id:int}/users/{userId}")]
        public async Task<IActionResult> RemoveUserFromWorkbench(int id, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wb = await _service.GetWorkbench(id);
            if (wb == null)
                return NotFound();

            if (wb.OwnerId == userId)
                return Forbid("You cannot remove the owner.");

            var currentMembership = await _service.GetMembership(id, currentUserId);
            if (currentMembership == null || currentMembership.Role != WorkbenchMember.WorkbenchRole.Admin)
                return Forbid();

            var removed = await _service.RemoveUserFromWorkbench(id, userId);
            if (!removed)
                return NotFound();

            return NoContent();
        }


        [HttpPut("{id:int}/users/{userId}/role/{role}")]
        public async Task<IActionResult> UpdateUserRole(int id, string userId, WorkbenchMember.WorkbenchRole role)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var wb = await _service.GetWorkbench(id);
            if (wb == null)
                return NotFound();
            var memberShip = await _service.GetMembership(id, currentUserId);

            if (memberShip == null || memberShip.Role != WorkbenchMember.WorkbenchRole.Admin)
            {
                return Forbid();
            }
            var updated = await _service.UpdateUserRole(id, userId, role);

            if (!updated)
            {
                return NotFound();
            }

            return Ok();
        }
    }

}
