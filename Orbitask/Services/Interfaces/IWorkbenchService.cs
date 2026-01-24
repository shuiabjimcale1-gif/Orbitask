using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IWorkbenchService
    {
        Task<Workbench?> GetWorkbench(int workbenchId);
        Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId);
        Task<Workbench?> CreateWorkbench(string userId, Workbench newWorkbench);
        Task<Workbench?> UpdateWorkbench(int workbenchId, Workbench updated);
        Task<bool> DeleteWorkbench(int workbenchId);

        Task<WorkbenchMember?> GetMembership(int workbenchId, string userId);
        Task<IEnumerable<WorkbenchMember>> GetMembers(int workbenchId);
        Task<bool> InviteMember(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<bool> UpdateMemberRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<bool> RemoveMember(int workbenchId, string userId);
    }
}