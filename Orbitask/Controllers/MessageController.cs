using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;  // 🆕 ADD THIS
using Orbitask.Hubs;                  // 🆕 ADD THIS
using Orbitask.Models;
using Orbitask.Services.Chats.Interfaces;
using Orbitask.Services.Interfaces;
using System.Security.Claims;

namespace Orbitask.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/messages")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private readonly IWorkbenchService _workbenchService;
        private readonly IHubContext<ChatHub> _hubContext;  // 🆕 ADD THIS

        public MessageController(
            IMessageService messageService,
            IChatService chatService,
            IWorkbenchService workbenchService,
            IHubContext<ChatHub> hubContext)  // 🆕 ADD THIS PARAMETER
        {
            _messageService = messageService;
            _chatService = chatService;
            _workbenchService = workbenchService;
            _hubContext = hubContext;  // 🆕 STORE IT
        }

        // ============================================
        // GET MESSAGES (No changes needed)
        // ============================================
        [HttpGet("/api/chats/{chatId}/messages")]
        public async Task<IActionResult> GetMessages(int chatId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Security checks (existing)
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            var chatMembership = await _chatService.GetChatMembership(chatId, userId);
            if (chatMembership == null) return Forbid();

            var messages = await _messageService.GetMessagesForChat(chatId);
            return Ok(messages);
        }

        // ============================================
        // SEND MESSAGE (SignalR added here!)
        // ============================================
        [HttpPost("/api/chats/{chatId}/messages")]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] Message newMessage)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var chat = await _chatService.GetChat(chatId);
            if (chat == null) return NotFound();

            // 🔒 Security checks (existing - keep as is)
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            var chatMembership = await _chatService.GetChatMembership(chatId, userId);
            if (chatMembership == null) return Forbid();

            // Save to database via your existing Service → Data flow
            var message = await _messageService.CreateMessage(chatId, userId, newMessage);
            if (message == null) return BadRequest();

            // 🆕 NEW: Push message to all users in this chat via SignalR
            await _hubContext.Clients
                .Group($"chat-{chatId}")           // Send to this specific chat group
                .SendAsync("ReceiveMessage", new   // Call "ReceiveMessage" on clients
                {
                    id = message.Id,
                    chatId = message.ChatId,
                    userId = message.UserId,
                    content = message.Content,
                    createdAt = message.CreatedAt
                });

            return Ok(message);
        }

        // ============================================
        // UPDATE MESSAGE (Add SignalR here too!)
        // ============================================
        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(int messageId, [FromBody] Message updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _messageService.GetMessage(messageId);
            if (message == null) return NotFound();

            var chat = await _chatService.GetChat(message.ChatId);

            // 🔒 Security checks (existing)
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            var chatMembership = await _chatService.GetChatMembership(message.ChatId, userId);
            if (chatMembership == null) return Forbid();

            var updatedMessage = await _messageService.UpdateMessage(messageId, userId, updated);
            if (updatedMessage == null) return Forbid();

            // 🆕 NEW: Notify all users that message was edited
            await _hubContext.Clients
                .Group($"chat-{message.ChatId}")
                .SendAsync("MessageUpdated", new
                {
                    id = updatedMessage.Id,
                    chatId = updatedMessage.ChatId,
                    content = updatedMessage.Content
                });

            return Ok(updatedMessage);
        }

        // ============================================
        // DELETE MESSAGE (Add SignalR here too!)
        // ============================================
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _messageService.GetMessage(messageId);
            if (message == null) return NotFound();

            var chat = await _chatService.GetChat(message.ChatId);

            // 🔒 Security checks (existing)
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            var chatMembership = await _chatService.GetChatMembership(message.ChatId, userId);
            if (chatMembership == null) return Forbid();

            var success = await _messageService.DeleteMessage(messageId, userId);
            if (!success) return Forbid();

            // 🆕 NEW: Notify all users that message was deleted
            await _hubContext.Clients
                .Group($"chat-{message.ChatId}")
                .SendAsync("MessageDeleted", new
                {
                    id = messageId,
                    chatId = message.ChatId
                });

            return NoContent();
        }
    }
}