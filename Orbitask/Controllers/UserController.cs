using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orbitask.Models;
using Orbitask.Models.ModelDtos;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    /// <summary>
    /// User Controller - "Backwards but Tenancy Protected"
    /// 
    /// IDs are exposed (needed for frontend operations), but all data access
    /// is protected by workbench membership checks. Users can only see/search
    /// users within workbenches they're members of.
    /// 
    /// Features:
    /// - Get users in workbench (with optional search)
    /// - Search users for invitation (excludes existing members)
    /// - Batch get users by IDs (performance optimization)
    /// - Get single user (workbench scoped)
    /// - Current user profile management
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IWorkbenchService _workbenchService;

        public UserController(UserManager<User> userManager, IWorkbenchService workbenchService)
        {
            _userManager = userManager;
            _workbenchService = workbenchService;
        }

        // ============================================
        // GET USERS IN WORKBENCH (WITH OPTIONAL SEARCH)
        // ============================================

        /// <summary>
        /// GET /api/workbenches/{workbenchId}/users?search=john
        /// 
        /// Returns user info for all members in a workbench.
        /// Supports optional search by name/email/username.
        /// 
        /// 🔒 TENANCY PROTECTED: Only returns users in the same workbench
        /// 
        /// Use cases:
        /// - Member directory
        /// - User listings
        /// - Search within workbench members
        /// </summary>
        [HttpGet("workbenches/{workbenchId:int}/users")]
        public async Task<IActionResult> GetUsersInWorkbench(
            int workbenchId,
            [FromQuery] string? search = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Check membership
                var membership = await _workbenchService.GetMembership(workbenchId, userId);
                if (membership == null)
                    return Forbid();

                // Get all member UserIds for this workbench
                var members = await _workbenchService.GetMembers(workbenchId);
                var memberUserIds = members.Select(m => m.UserId).ToList();

                // Query users with optional search filter
                var query = _userManager.Users.Where(u => memberUserIds.Contains(u.Id));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(u =>
                        u.DisplayName.ToLower().Contains(searchLower) ||
                        u.UserName.ToLower().Contains(searchLower) ||
                        u.Email.ToLower().Contains(searchLower)
                    );
                }

                var users = await query
                    .Select(u => new UserInfoDto
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        UserName = u.UserName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl
                    })
                    .OrderBy(u => u.DisplayName)
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving users" });
            }
        }

        // ============================================
        // SEARCH USERS FOR INVITATION
        // ============================================

        /// <summary>
        /// GET /api/workbenches/{workbenchId}/users/search?query=alice&limit=10
        /// 
        /// Search for users to invite to workbench.
        /// Returns users NOT already in the workbench.
        /// 
        /// 🔒 TENANCY PROTECTED: Only accessible by workbench members
        /// 
        /// Use cases:
        /// - Invite member dropdown/autocomplete
        /// - "Add user" search boxes
        /// - Finding users to collaborate with
        /// </summary>
        [HttpGet("workbenches/{workbenchId:int}/users/search")]
        public async Task<IActionResult> SearchUsersForInvite(
            int workbenchId,
            [FromQuery] string query,
            [FromQuery] int limit = 10)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Must be member to search for invites
                var membership = await _workbenchService.GetMembership(workbenchId, userId);
                if (membership == null)
                    return Forbid();

                // Validate query
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                    return BadRequest(new { error = "Search query must be at least 2 characters" });

                // Validate limit
                if (limit < 1 || limit > 50)
                    limit = 10;

                // Get existing member IDs
                var members = await _workbenchService.GetMembers(workbenchId);
                var existingMemberIds = members.Select(m => m.UserId).ToList();

                // Search users NOT in workbench
                var searchLower = query.ToLower();
                var users = await _userManager.Users
                    .Where(u =>
                        !existingMemberIds.Contains(u.Id) &&
                        (u.DisplayName.ToLower().Contains(searchLower) ||
                         u.UserName.ToLower().Contains(searchLower) ||
                         u.Email.ToLower().Contains(searchLower))
                    )
                    .Take(limit)
                    .Select(u => new UserSearchResultDto
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        UserName = u.UserName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while searching users" });
            }
        }

        // ============================================
        // BATCH GET USERS BY IDS
        // ============================================

        /// <summary>
        /// POST /api/workbenches/{workbenchId}/users/batch
        /// Body: { "userIds": ["id1", "id2", "id3"] }
        /// 
        /// Get multiple users by IDs in a single request.
        /// 
        /// 🔒 TENANCY PROTECTED: Only returns users in the workbench
        /// 
        /// Use cases:
        /// - Loading user details for tasks (created by, assigned to)
        /// - Display authors of multiple messages
        /// - Bulk user info retrieval
        /// - Frontend caching optimization
        /// 
        /// Limits:
        /// - Max 100 user IDs per request
        /// - Only returns users who are members of the workbench
        /// </summary>
        [HttpPost("workbenches/{workbenchId:int}/users/batch")]
        public async Task<IActionResult> GetUsersBatch(
            int workbenchId,
            [FromBody] BatchUserRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Check membership
                var membership = await _workbenchService.GetMembership(workbenchId, userId);
                if (membership == null)
                    return Forbid();

                // Validate request
                if (request.UserIds == null || !request.UserIds.Any())
                    return BadRequest(new { error = "UserIds array is required" });

                if (request.UserIds.Count > 100)
                    return BadRequest(new { error = "Maximum 100 users per batch request" });

                // Get workbench member IDs
                var members = await _workbenchService.GetMembers(workbenchId);
                var memberUserIds = members.Select(m => m.UserId).ToHashSet();

                // Filter requested IDs to only those in the workbench
                var validUserIds = request.UserIds
                    .Where(id => memberUserIds.Contains(id))
                    .ToList();

                // Get users
                var users = await _userManager.Users
                    .Where(u => validUserIds.Contains(u.Id))
                    .Select(u => new UserInfoDto
                    {
                        Id = u.Id,
                        DisplayName = u.DisplayName,
                        UserName = u.UserName,
                        Email = u.Email,
                        AvatarUrl = u.AvatarUrl
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving users" });
            }
        }

        // ============================================
        // GET SINGLE USER IN WORKBENCH
        // ============================================

        /// <summary>
        /// GET /api/workbenches/{workbenchId}/users/{targetUserId}
        /// 
        /// Returns user info for a specific user in a workbench.
        /// 
        /// 🔒 TENANCY PROTECTED: Both requester and target must be in workbench
        /// 
        /// Use cases:
        /// - Load profile details
        /// - Display user info in modals/popovers
        /// - Verify user exists in workbench
        /// </summary>
        [HttpGet("workbenches/{workbenchId:int}/users/{targetUserId}")]
        public async Task<IActionResult> GetUserInWorkbench(int workbenchId, string targetUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                // 🔒 TENANCY WALL: Check requester membership
                var membership = await _workbenchService.GetMembership(workbenchId, userId);
                if (membership == null)
                    return Forbid();

                // 🔒 TENANCY WALL: Verify target user is in workbench
                var targetMembership = await _workbenchService.GetMembership(workbenchId, targetUserId);
                if (targetMembership == null)
                    return NotFound("User not found in workbench");

                // Get user info
                var user = await _userManager.FindByIdAsync(targetUserId);
                if (user == null)
                    return NotFound("User not found");

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    AvatarUrl = user.AvatarUrl
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving user" });
            }
        }

        // ============================================
        // GET CURRENT USER PROFILE
        // ============================================

        /// <summary>
        /// GET /api/users/me
        /// 
        /// Returns current authenticated user's profile.
        /// 
        /// Use cases:
        /// - Load logged-in user's profile
        /// - Display user info in header/navigation
        /// - Profile settings page initialization
        /// </summary>
        [HttpGet("users/me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    AvatarUrl = user.AvatarUrl
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving user profile" });
            }
        }

        // ============================================
        // UPDATE CURRENT USER PROFILE
        // ============================================

        /// <summary>
        /// PUT /api/users/me
        /// Body: { "displayName": "New Name", "avatarUrl": "https://..." }
        /// 
        /// Updates current user's profile (DisplayName, AvatarUrl).
        /// User can only update their own profile.
        /// 
        /// Use cases:
        /// - Profile settings page
        /// - Update display name
        /// - Change avatar
        /// </summary>
        [HttpPut("users/me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return NotFound("User not found");

                user.DisplayName = updateDto.DisplayName;
                user.AvatarUrl = updateDto.AvatarUrl;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return BadRequest(new { error = "Failed to update user profile" });

                var userDto = new UserInfoDto
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    AvatarUrl = user.AvatarUrl
                };

                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while updating user profile" });
            }
        }
    }
}