using Orbitask.Models;

namespace Orbitask.Data.Chats.Interfaces
{
    public interface IMessageData
    {
        Task<IEnumerable<Message>> GetMessagesForChat(int chatId, int limit = 50);
        Task<Message?> GetMessage(int messageId);
        Task<Message> InsertMessage(Message message);
        Task<Message?> UpdateMessage(Message message);
        Task<bool> DeleteMessage(int messageId);
        Task<int?> GetChatIdForMessage(int messageId);
    }
}