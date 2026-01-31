using Orbitask.Models;

namespace Orbitask.Services.Chats.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<Message>> GetMessagesForChat(int chatId);
        Task<Message?> GetMessage(int messageId);
        Task<Message?> CreateMessage(int chatId, string userId, Message newMessage);
        Task<Message?> UpdateMessage(int messageId, string userId, Message updated);
        Task<bool> DeleteMessage(int messageId, string userId);
    }
}