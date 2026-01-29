namespace Orbitask.Models
{
    public class ChatUser
    {
        public enum ChatRole
        {
            Admin = 0,
            Member = 1
        }

        public int ChatId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ChatRole? Role { get; set; }  // NULL for direct chats
        public DateTime JoinedAt { get; set; }
    }
}