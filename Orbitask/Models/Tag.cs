namespace Orbitask.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        // FK's
        public int BoardId { get; set; }
        public int WorkbenchId { get; set; }
    }
}
