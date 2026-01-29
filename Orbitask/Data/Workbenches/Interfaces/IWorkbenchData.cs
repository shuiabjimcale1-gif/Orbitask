using Orbitask.Models;

namespace Orbitask.Data.Workbenches.Interfaces
{
    public interface IWorkbenchData
    {
        // Workbench CRUD
        Task<Workbench?> GetWorkbench(int workbenchId);
        Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId);
        Task<Workbench> InsertWorkbench(Workbench workbench);
        Task<Workbench?> UpdateWorkbench(Workbench workbench);
        Task<bool> DeleteWorkbench(int workbenchId);

        // Membership (the "two birds" method)
        Task<WorkbenchMember?> GetMembership(int workbenchId, string userId);
        Task<IEnumerable<WorkbenchMember>> GetMembersForWorkbench(int workbenchId);
        Task<bool> AddMember(WorkbenchMember member);
        Task<bool> UpdateMemberRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<bool> RemoveMember(int workbenchId, string userId);

        // Helpers
        Task<bool> WorkbenchExists(int workbenchId);
        Task<bool> UserExists(string userId);
    }
}