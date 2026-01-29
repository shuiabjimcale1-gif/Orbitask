using Orbitask.Models;

namespace Orbitask.Data.Chats.Interfaces
{
    public interface IChatData
    {
        // CRUD
        Task<IEnumerable<Chat>> GetChatsForUser(string userId, int workbenchId);
        Task<Chat?> GetChat(int chatId);
        Task<Chat> InsertChat(Chat chat);
        Task<Chat?> UpdateChat(Chat chat);
        Task<bool> DeleteChat(int chatId);

        // Helpers
        Task<int?> GetWorkbenchIdForChat(int chatId);
        Task UpdateLastMessageAt(int chatId, DateTime timestamp);
        Task<bool> ChatExists(int chatId);
    }
}