namespace Orbitask.Models
{
    public class WorkbenchMember
    {
        public enum WorkbenchRole
        {
            Admin = 1,
            Member = 2
        }

        public string UserId { get; set; }
        public int WorkbenchId { get; set; }
        public WorkbenchRole Role { get; set; }
    }
}
