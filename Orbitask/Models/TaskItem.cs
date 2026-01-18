namespace Orbitask.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? DueDate { get; set; }

        // FK's
        public int ColumnId { get; set; }
        public int BoardId { get; set; }
        public int WorkbenchId { get; set; }

    }
}
