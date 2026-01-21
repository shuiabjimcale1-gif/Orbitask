using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class WorkbenchService : IWorkbenchService
    {
        private readonly IWorkbenchData _data;

        public WorkbenchService(IWorkbenchData data)
        {
            _data = data;
        }

        public async Task<Workbench?> GetWorkbench(int id)
        {
            return await _data.GetWorkbench(id);
        }

        public async Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId)
        {
            return await _data.GetWorkbenchesForUser(userId);
        }

        public async Task<Workbench?> CreateWorkbench(string userId, Workbench workbench)
        {
            return await _data.InsertWorkbench(userId, workbench);
        }

        public async Task<Workbench?> UpdateWorkbench(int id, Workbench updated)
        {

            updated.Id = id;

            var success = await _data.UpdateWorkbench(updated);
            return success ? updated : null;
        }

        public async Task<bool> DeleteWorkbench(int id, string userId)
        {
            return await _data.DeleteWorkbench(id);
        }
        public async Task<IEnumerable<string>?> GetUsersForWorkbench(int workbenchId)
        {
            

            var users = await _data.GetUsersForWorkbench(workbenchId);

            if (users == null)
            {
                return null;
            }

            return users;
        }

        public async Task<bool> AddUserToWorkbench(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            

            var added = await _data.AddUserToWorkbench(workbenchId, userId, role);

            if (!added)
            {
                return false;
            }

            return true;
        }


        public async Task<bool> RemoveUserFromWorkbench(int workbenchId, string userId)
        {
            var wb = await _data.GetWorkbench(workbenchId);

            if (wb == null)
            {
                return false;
            }

            var removed = await _data.RemoveUserFromWorkbench(workbenchId, userId);

            if (!removed)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateUserRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {

            var updated = await _data.UpdateUserRole(workbenchId, userId, role);

            if (!updated)
            {
                return false;
            }

            return true;
        }

        public async Task<WorkbenchMember?> GetMembership(int workbenchId, string userId)
        {
            return await _data.GetMembership(workbenchId, userId);
        }
    }

}
