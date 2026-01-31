using Orbitask.Data.Chats.Interfaces;
using Orbitask.Data.Workbenches.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Chats.Interfaces;

namespace Orbitask.Services.Chats
{
    public class ChatService : IChatService
    {
        private readonly IChatData _chatData;
        private readonly IChatUserData _chatUserData;
        private readonly IWorkbenchData _workbenchData;

        public ChatService(
            IChatData chatData,
            IChatUserData chatUserData,
            IWorkbenchData workbenchData)
        {
            _chatData = chatData;
            _chatUserData = chatUserData;
            _workbenchData = workbenchData;
        }

        public async Task<IEnumerable<Chat>> GetChatsForUser(string userId, int workbenchId)
        {
            return await _chatData.GetChatsForUser(userId, workbenchId);
        }

        public async Task<Chat?> GetChat(int chatId)
        {
            return await _chatData.GetChat(chatId);
        }

        public async Task<Chat?> CreateDirectChat(int workbenchId, string userId1, string userId2)
        {
            // Validate workbench exists
            if (!await _workbenchData.WorkbenchExists(workbenchId))
                return null;

            // Validate both users are in workbench
            var user1Membership = await _workbenchData.GetMembership(workbenchId, userId1);
            var user2Membership = await _workbenchData.GetMembership(workbenchId, userId2);

            if (user1Membership == null || user2Membership == null)
                return null;

            // Create chat
            var chat = new Chat
            {
                Type = Chat.ChatType.Direct,
                WorkbenchId = workbenchId,
                Name = null,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = null
            };

            chat = await _chatData.InsertChat(chat);

            // Add both users (no roles for direct chats)
            await _chatUserData.AddMember(new ChatUser
            {
                ChatId = chat.Id,
                UserId = userId1,
                Role = null,
                JoinedAt = DateTime.UtcNow
            });

            await _chatUserData.AddMember(new ChatUser
            {
                ChatId = chat.Id,
                UserId = userId2,
                Role = null,
                JoinedAt = DateTime.UtcNow
            });

            return chat;
        }

        public async Task<Chat?> CreateGroupChat(
            int workbenchId,
            string creatorId,
            Chat newChat,
            List<string> memberIds)
        {
            // Validate workbench exists
            if (!await _workbenchData.WorkbenchExists(workbenchId))
                return null;

            // Validate creator is in workbench
            var creatorMembership = await _workbenchData.GetMembership(workbenchId, creatorId);
            if (creatorMembership == null)
                return null;

            // Validate all members are in workbench
            foreach (var memberId in memberIds)
            {
                var membership = await _workbenchData.GetMembership(workbenchId, memberId);
                if (membership == null)
                    return null;
            }

            // Create chat
            var chat = new Chat
            {
                Type = Chat.ChatType.Group,
                WorkbenchId = workbenchId,
                Name = newChat.Name,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = null
            };

            chat = await _chatData.InsertChat(chat);

            // Add creator as Admin
            await _chatUserData.AddMember(new ChatUser
            {
                ChatId = chat.Id,
                UserId = creatorId,
                Role = ChatUser.ChatRole.Admin,
                JoinedAt = DateTime.UtcNow
            });

            // Add other members
            foreach (var memberId in memberIds)
            {
                if (memberId != creatorId)
                {
                    await _chatUserData.AddMember(new ChatUser
                    {
                        ChatId = chat.Id,
                        UserId = memberId,
                        Role = ChatUser.ChatRole.Member,
                        JoinedAt = DateTime.UtcNow
                    });
                }
            }

            return chat;
        }

        public async Task<Chat?> UpdateChat(int chatId, string userId, Chat updated)
        {
            var existing = await _chatData.GetChat(chatId);
            if (existing == null) return null;

            // Can only update group chats
            if (existing.Type == Chat.ChatType.Direct)
                return null;

            // Only Admin can update
            var membership = await _chatUserData.GetChatMembership(chatId, userId);
            if (membership?.Role != ChatUser.ChatRole.Admin)
                return null;

            // Update only name
            updated.Id = chatId;
            updated.Type = existing.Type;
            updated.WorkbenchId = existing.WorkbenchId;
            updated.CreatedAt = existing.CreatedAt;
            updated.LastMessageAt = existing.LastMessageAt;

            return await _chatData.UpdateChat(updated);
        }

        public async Task<bool> DeleteChat(int chatId, string userId)
        {
            var chat = await _chatData.GetChat(chatId);
            if (chat == null) return false;

            // For group chats, only Admin can delete
            if (chat.Type == Chat.ChatType.Group)
            {
                var membership = await _chatUserData.GetChatMembership(chatId, userId);
                if (membership?.Role != ChatUser.ChatRole.Admin)
                    return false;
            }
            // For direct chats, either user can delete
            else
            {
                var isMember = await _chatUserData.IsMember(chatId, userId);
                if (!isMember)
                    return false;
            }

            return await _chatData.DeleteChat(chatId);
        }

        public async Task<ChatUser?> GetChatMembership(int chatId, string userId)
        {
            return await _chatUserData.GetChatMembership(chatId, userId);
        }

        public async Task<IEnumerable<ChatUser>> GetChatMembers(int chatId)
        {
            return await _chatUserData.GetMembersForChat(chatId);
        }
    }
}