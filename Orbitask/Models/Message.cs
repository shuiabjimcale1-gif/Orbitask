namespace Orbitask.Models
{
    /// <summary>
    /// Represents a single message in a chat
    /// </summary>
    public class Message
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}