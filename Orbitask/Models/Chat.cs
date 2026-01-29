namespace Orbitask.Models
{
    public class Chat
    {
        public enum ChatType
        {
            Direct = 0,
            Group = 1
        }

        public int Id { get; set; }
        public ChatType Type { get; set; }
        public int WorkbenchId { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}