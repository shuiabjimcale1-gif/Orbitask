using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IWorkbenchService
    {
        Task<Workbench?> GetWorkbench(int id);
        Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId);

        Task<Workbench?> CreateWorkbench(string userId,Workbench workbench);
        Task<Workbench?> UpdateWorkbench(int id, Workbench updated);
        Task<bool> DeleteWorkbench(int id, string userId);

        Task<IEnumerable<string>?> GetUsersForWorkbench(int workbenchId);
        Task<bool> AddUserToWorkbench(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<bool> RemoveUserFromWorkbench(int workbenchId, string userId);
        Task<bool> UpdateUserRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<WorkbenchMember?> GetMembership(int workbenchId, string userId);

    }

}
