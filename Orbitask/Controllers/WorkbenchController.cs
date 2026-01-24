using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IWorkbenchService _workbenchService;

        public WorkbenchController(IWorkbenchService workbenchService)
        {
            _workbenchService = workbenchService;
        }

        // GET /api/workbenches
        [HttpGet]
        public async Task<IActionResult> GetMyWorkbenches()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workbenches = await _workbenchService.GetWorkbenchesForUser(userId);
            return Ok(workbenches);
        }

        // GET /api/workbenches/{id}
        [HttpGet("{workbenchId:int}")]
        public async Task<IActionResult> GetWorkbench(int workbenchId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var workbench = await _workbenchService.GetWorkbench(workbenchId);
            if (workbench == null) return NotFound();

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            return Ok(workbench);
        }

        // POST /api/workbenches
        [HttpPost]
        public async Task<IActionResult> CreateWorkbench([FromBody] Workbench newWorkbench)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workbench = await _workbenchService.CreateWorkbench(userId, newWorkbench);
            return Ok(workbench);
        }

        // PUT /api/workbenches/{id}
        [HttpPut("{workbenchId:int}")]
        public async Task<IActionResult> UpdateWorkbench(int workbenchId, [FromBody] Workbench updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null || membership.Role == WorkbenchMember.WorkbenchRole.Member)
                return Forbid();

            var workbench = await _workbenchService.UpdateWorkbench(workbenchId, updated);
            if (workbench == null) return NotFound();

            return Ok(workbench);
        }

        // DELETE /api/workbenches/{id} - OWNER ONLY
        [HttpDelete("{workbenchId:int}")]
        public async Task<IActionResult> DeleteWorkbench(int workbenchId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership?.Role != WorkbenchMember.WorkbenchRole.Owner)
                return Forbid();  // Only Owner can delete!

            var success = await _workbenchService.DeleteWorkbench(workbenchId);
            if (!success) return NotFound();

            return NoContent();
        }

        // GET /api/workbenches/{id}/members
        [HttpGet("{workbenchId:int}/members")]
        public async Task<IActionResult> GetMembers(int workbenchId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            var members = await _workbenchService.GetMembers(workbenchId);
            return Ok(members);
        }

        // POST /api/workbenches/{id}/members
        [HttpPost("{workbenchId:int}/members")]
        public async Task<IActionResult> InviteMember(int workbenchId, [FromBody] WorkbenchMember newMember)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership?.Role == WorkbenchMember.WorkbenchRole.Member)
                return Forbid();

            // 🔒 Cannot invite as Owner
            if (newMember.Role == WorkbenchMember.WorkbenchRole.Owner)
                return BadRequest(new { error = "Cannot invite as Owner" });

            var success = await _workbenchService.InviteMember(workbenchId, newMember.UserId, newMember.Role);
            if (!success) return BadRequest(new { error = "Failed to invite member" });

            return Ok(new { message = "Member invited" });
        }

        // PUT /api/workbenches/{id}/members/{userId}
        [HttpPut("{workbenchId:int}/members/{targetUserId}")]
        public async Task<IActionResult> UpdateMemberRole(
            int workbenchId,
            string targetUserId,
            [FromBody] WorkbenchMember.WorkbenchRole newRole)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership?.Role == WorkbenchMember.WorkbenchRole.Member)
                return Forbid();

            var targetMembership = await _workbenchService.GetMembership(workbenchId, targetUserId);
            if (targetMembership == null) return NotFound();

            // 🔒 Cannot promote to Owner
            if (newRole == WorkbenchMember.WorkbenchRole.Owner)
                return BadRequest(new { error = "Cannot promote to Owner" });

            // 🔒 Cannot demote Owner
            if (targetMembership.Role == WorkbenchMember.WorkbenchRole.Owner)
                return BadRequest(new { error = "Cannot change Owner's role" });

            // 🔒 Cannot change own role
            if (targetUserId == userId)
                return BadRequest(new { error = "Cannot change your own role" });

            var success = await _workbenchService.UpdateMemberRole(workbenchId, targetUserId, newRole);
            if (!success) return NotFound();

            return Ok(new { message = "Role updated" });
        }

        // DELETE /api/workbenches/{id}/members/{userId} - AUTO SUCCESSION
        [HttpDelete("{workbenchId:int}/members/{targetUserId}")]
        public async Task<IActionResult> RemoveMember(int workbenchId, string targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            var targetMembership = await _workbenchService.GetMembership(workbenchId, targetUserId);
            if (targetMembership == null) return NotFound();

           
            if (targetMembership.Role == WorkbenchMember.WorkbenchRole.Owner)
            {
                if (targetUserId != userId)
                    return BadRequest(new { error = "Only Owner can leave" });

                var members = await _workbenchService.GetMembers(workbenchId);
                var nextOwner = members
                    .Where(m => m.UserId != userId && m.Role == WorkbenchMember.WorkbenchRole.Admin)
                    .FirstOrDefault();

                if (nextOwner == null)
                    return BadRequest(new { error = "Promote someone to Admin first" });

                // Promote Admin to Owner
                await _workbenchService.UpdateMemberRole(
                    workbenchId, nextOwner.UserId, WorkbenchMember.WorkbenchRole.Owner);

                await _workbenchService.RemoveMember(workbenchId, targetUserId);

                return Ok(new { message = "Ownership transferred", newOwnerId = nextOwner.UserId });
            }

            // Regular member removal
            if (membership.Role == WorkbenchMember.WorkbenchRole.Member && targetUserId != userId)
                return Forbid();

            await _workbenchService.RemoveMember(workbenchId, targetUserId);
            return NoContent();
        }
    }
}