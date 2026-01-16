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
            workbench.OwnerId = userId;
            return await _data.InsertWorkbench(workbench);
        }

        public async Task<Workbench?> UpdateWorkbench(int id, Workbench updated)
        {
            if (!await _data.WorkbenchExists(id))
                return null;

            updated.Id = id;

            var success = await _data.UpdateWorkbench(updated);
            return success ? updated : null;
        }

        public async Task<bool> DeleteWorkbench(int id, string userId)
        {
            if (!await _data.UserOwnsWorkbench(id, userId))
                return false;

            return await _data.DeleteWorkbench(id);
        }
    }

}
