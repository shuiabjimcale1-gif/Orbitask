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
    [Route("api/messages")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        private readonly IChatService _chatService;
        private readonly IWorkbenchService _workbenchService;

        public MessageController(
            IMessageService messageService,
            IChatService chatService,
            IWorkbenchService workbenchService)
        {
            _messageService = messageService;
            _chatService = chatService;
            _workbenchService = workbenchService;
        }

        /// <summary>
        /// GET /api/chats/{chatId}/messages
        /// Get all messages in a chat
        /// </summary>
        [HttpGet("/api/chats/{chatId}/messages")]
        public async Task<IActionResult> GetMessages(int chatId)
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

            var messages = await _messageService.GetMessagesForChat(chatId);
            return Ok(messages);
        }

        /// <summary>
        /// POST /api/chats/{chatId}/messages
        /// Send a message in a chat
        /// </summary>
        [HttpPost("/api/chats/{chatId}/messages")]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] Message newMessage)
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

            var message = await _messageService.CreateMessage(chatId, userId, newMessage);
            if (message == null) return BadRequest();

            return Ok(message);
        }

        /// <summary>
        /// PUT /api/messages/{messageId}
        /// Edit your own message
        /// </summary>
        [HttpPut("{messageId}")]
        public async Task<IActionResult> UpdateMessage(int messageId, [FromBody] Message updated)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _messageService.GetMessage(messageId);
            if (message == null) return NotFound();

            var chat = await _chatService.GetChat(message.ChatId);

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            // 🔒 Check chat membership
            var chatMembership = await _chatService.GetChatMembership(message.ChatId, userId);
            if (chatMembership == null) return Forbid();

            var updatedMessage = await _messageService.UpdateMessage(messageId, userId, updated);
            if (updatedMessage == null) return Forbid();

            return Ok(updatedMessage);
        }

        /// <summary>
        /// DELETE /api/messages/{messageId}
        /// Delete message (own message or Admin)
        /// </summary>
        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var message = await _messageService.GetMessage(messageId);
            if (message == null) return NotFound();

            var chat = await _chatService.GetChat(message.ChatId);

            // 🔒 Check workbench membership
            var workbenchMembership = await _workbenchService.GetMembership(chat.WorkbenchId, userId);
            if (workbenchMembership == null) return Forbid();

            // 🔒 Check chat membership
            var chatMembership = await _chatService.GetChatMembership(message.ChatId, userId);
            if (chatMembership == null) return Forbid();

            var success = await _messageService.DeleteMessage(messageId, userId);
            if (!success) return Forbid();

            return NoContent();
        }
    }
}