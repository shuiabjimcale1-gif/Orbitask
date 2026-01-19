using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface IWorkbenchData
    {
        Task<Workbench?> GetWorkbench(int id);
        Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId);

        Task<Workbench> InsertWorkbench(Workbench workbench);
        Task<bool> UpdateWorkbench(Workbench workbench);
        Task<bool> DeleteWorkbench(int id);

        Task<bool> WorkbenchExists(int id);
        Task<bool> UserOwnsWorkbench(int id, string userId);
        Task<IEnumerable<string>> GetUsersForWorkbench(int workbenchId);
        Task<bool> AddUserToWorkbench(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);
        Task<bool> RemoveUserFromWorkbench(int workbenchId, string userId);
        Task<bool> UpdateUserRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role);


    }

}