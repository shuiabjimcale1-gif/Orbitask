namespace Orbitask.Models
{
    public class TaskTag
    {
        // FK's

        public int TaskItemId { get; set; }
        public int TagId { get; set; }
        public int WorkbenchId { get; set; }
    }
}