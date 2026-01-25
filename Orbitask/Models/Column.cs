namespace Orbitask.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Position { get; set; }

        // FK's
        public int BoardId { get; set; }
    }
}
