using Orbitask.Data.Chats.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Chats.Interfaces;

namespace Orbitask.Services.Chats
{
    public class MessageService : IMessageService
    {
        private readonly IMessageData _messageData;
        private readonly IChatData _chatData;
        private readonly IChatUserData _chatUserData;

        public MessageService(
            IMessageData messageData,
            IChatData chatData,
            IChatUserData chatUserData)
        {
            _messageData = messageData;
            _chatData = chatData;
            _chatUserData = chatUserData;
        }

        public async Task<IEnumerable<Message>> GetMessagesForChat(int chatId)
        {
            return await _messageData.GetMessagesForChat(chatId);
        }

        public async Task<Message?> GetMessage(int messageId)
        {
            return await _messageData.GetMessage(messageId);
        }

        public async Task<Message?> CreateMessage(int chatId, string userId, Message newMessage)
        {
            // Validate chat exists
            var chat = await _chatData.GetChat(chatId);
            if (chat == null) return null;

            // Validate user is in chat
            var isMember = await _chatUserData.IsMember(chatId, userId);
            if (!isMember) return null;

            // Set properties
            newMessage.ChatId = chatId;
            newMessage.UserId = userId;
            newMessage.CreatedAt = DateTime.UtcNow;

            // Insert message
            var message = await _messageData.InsertMessage(newMessage);

            // Update chat's LastMessageAt
            await _chatData.UpdateLastMessageAt(chatId, DateTime.UtcNow);

            return message;
        }

        public async Task<Message?> UpdateMessage(int messageId, string userId, Message updated)
        {
            var existing = await _messageData.GetMessage(messageId);
            if (existing == null) return null;

            // Can only edit own message
            if (existing.UserId != userId)
                return null;

            // Update only content
            updated.Id = messageId;
            updated.ChatId = existing.ChatId;
            updated.UserId = existing.UserId;
            updated.CreatedAt = existing.CreatedAt;

            return await _messageData.UpdateMessage(updated);
        }

        public async Task<bool> DeleteMessage(int messageId, string userId)
        {
            var message = await _messageData.GetMessage(messageId);
            if (message == null) return false;

            // Get chat membership
            var membership = await _chatUserData.GetChatMembership(message.ChatId, userId);
            if (membership == null) return false;

            // Can delete if: Own message OR Admin
            if (message.UserId != userId && membership.Role != ChatUser.ChatRole.Admin)
                return false;

            return await _messageData.DeleteMessage(messageId);
        }
    }
}