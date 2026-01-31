using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services.Chats.Interfaces;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IWorkbenchService _workbenchService;

        public ChatController(
            IChatService chatService,
            IWorkbenchService workbenchService)
        {
            _chatService = chatService;
            _workbenchService = workbenchService;
        }

        /// <summary>
        /// GET /api/workbenches/{workbenchId}/chats
        /// Get all chats for current user in a workbench
        /// </summary>
        [HttpGet("/api/workbenches/{workbenchId}/chats")]
        public async Task<IActionResult> GetMyChats(int workbenchId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🔒 Check workbench membership
            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            var chats = await _chatService.GetChatsForUser(userId, workbenchId);
            return Ok(chats);
        }

        /// <summary>
        /// GET /api/chats/{chatId}
        /// Get single chat details
        /// </summary>
        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChat(int chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            // 🔒 Check chat membership
            var chatMembership = await _chatService.GetChatMembership(chatId, userId);
            if (chatMembership == null) return Forbid();

            return Ok(chat);
        }

        /// <summary>
        /// POST /api/workbenches/{workbenchId}/chats/direct
        /// Create a direct chat with another user
        /// </summary>
        [HttpPost("/api/workbenches/{workbenchId}/chats/direct")]
        public async Task<IActionResult> CreateDirectChat(
            int workbenchId,
            [FromBody] string recipientUserId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🔒 Check workbench membership
            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            var chat = await _chatService.CreateDirectChat(workbenchId, userId, recipientUserId);
            if (chat == null) return BadRequest(new { error = "Failed to create chat" });

            return Ok(chat);
        }

        /// <summary>
        /// POST /api/workbenches/{workbenchId}/chats/group
        /// Create a group chat with multiple users
        /// </summary>
        [HttpPost("/api/workbenches/{workbenchId}/chats/group")]
        public async Task<IActionResult> CreateGroupChat(
            int workbenchId,
            [FromBody] CreateGroupChatRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 🔒 Check workbench membership
            var membership = await _workbenchService.GetMembership(workbenchId, userId);
            if (membership == null) return Forbid();

            var chat = await _chatService.CreateGroupChat(
                workbenchId,
                userId,
                new Chat { Name = request.Name },
                request.MemberIds
            );

            if (chat == null) return BadRequest(new { error = "Failed to create group chat" });

            return Ok(chat);
        }

        /// <summary>
        /// PUT /api/chats/{chatId}
        /// Update group chat name (Admin only)
        /// </summary>
        [HttpPut("{chatId}")]
        public async Task<IActionResult> UpdateChat(int chatId, [FromBody] Chat updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            // 🔒 Check chat membership
            var chatMembership = await _chatService.GetChatMembership(chatId, userId);
            if (chatMembership == null) return Forbid();

            var updatedChat = await _chatService.UpdateChat(chatId, userId, updated);
            if (updatedChat == null) return Forbid();

            return Ok(updatedChat);
        }

        /// <summary>
        /// DELETE /api/chats/{chatId}
        /// Delete chat (Admin only for groups, any member for direct)
        /// </summary>
        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            var success = await _chatService.DeleteChat(chatId, userId);
            if (!success) return Forbid();

            return NoContent();
        }

        /// <summary>
        /// GET /api/chats/{chatId}/members
        /// Get all members of a chat
        /// </summary>
        [HttpGet("{chatId}/members")]
        public async Task<IActionResult> GetMembers(int chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            // 🔒 Check chat membership
            var chatMembership = await _chatService.GetChatMembership(chatId, userId);
            if (chatMembership == null) return Forbid();

            var members = await _chatService.GetChatMembers(chatId);
            return Ok(members);
        }
    }

    public class CreateGroupChatRequest
    {
        public string Name { get; set; } = string.Empty;
        public List<string> MemberIds { get; set; } = new();
    }
}