namespace Orbitask.Models
{
    public class Board
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // FK's
        public int WorkbenchId { get; set; }


    }
}
