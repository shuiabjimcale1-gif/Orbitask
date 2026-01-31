using Orbitask.Models;

namespace Orbitask.Services.Chats.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<Chat>> GetChatsForUser(string userId, int workbenchId);
        Task<Chat?> GetChat(int chatId);
        Task<Chat?> CreateDirectChat(int workbenchId, string userId1, string userId2);
        Task<Chat?> CreateGroupChat(int workbenchId, string creatorId, Chat newChat, List<string> memberIds);
        Task<Chat?> UpdateChat(int chatId, string userId, Chat updated);
        Task<bool> DeleteChat(int chatId, string userId);

        Task<ChatUser?> GetChatMembership(int chatId, string userId);
        Task<IEnumerable<ChatUser>> GetChatMembers(int chatId);
    }
}