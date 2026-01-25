namespace Orbitask.Models
{
    public class WorkbenchMember
    {
        public enum WorkbenchRole
        {
            Owner = 0,   // Creator, can delete, will be billed
            Admin = 1,   // Can manage, but can't delete
            Member = 2   // Can work, limited permissions
        }

        public int WorkbenchId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public WorkbenchRole Role { get; set; }
    }
}