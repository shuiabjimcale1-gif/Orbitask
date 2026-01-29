using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IChatUserData
    {
        Task<IEnumerable<ChatUser>> GetMembersForChat(int chatId);
        Task<ChatUser?> GetChatMembership(int chatId, string userId);
        Task<bool> AddMember(ChatUser member);
        Task<bool> UpdateMemberRole(int chatId, string userId, ChatUser.ChatRole role);
        Task<bool> RemoveMember(int chatId, string userId);
        Task<bool> IsMember(int chatId, string userId);
    }
}