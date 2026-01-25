using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class WorkbenchService : IWorkbenchService
    {
        private readonly IWorkbenchData _workbenchData;

        public WorkbenchService(IWorkbenchData workbenchData)
        {
            _workbenchData = workbenchData;
        }

        public async Task<Workbench?> GetWorkbench(int workbenchId)
        {
            return await _workbenchData.GetWorkbench(workbenchId);
        }

        public async Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId)
        {
            return await _workbenchData.GetWorkbenchesForUser(userId);
        }

        // ✅ Creator becomes Owner automatically
        public async Task<Workbench?> CreateWorkbench(string userId, Workbench newWorkbench)
        {
            var workbench = await _workbenchData.InsertWorkbench(newWorkbench);

            var membership = new WorkbenchMember
            {
                WorkbenchId = workbench.Id,
                UserId = userId,
                Role = WorkbenchMember.WorkbenchRole.Owner  // Not Admin!
            };

            await _workbenchData.AddMember(membership);
            return workbench;
        }

        public async Task<Workbench?> UpdateWorkbench(int workbenchId, Workbench updated)
        {
            var existing = await _workbenchData.GetWorkbench(workbenchId);
            if (existing == null) return null;

            updated.Id = workbenchId;
            return await _workbenchData.UpdateWorkbench(updated);
        }

        public async Task<bool> DeleteWorkbench(int workbenchId)
        {
            if (!await _workbenchData.WorkbenchExists(workbenchId))
                return false;
            return await _workbenchData.DeleteWorkbench(workbenchId);
        }

        public async Task<WorkbenchMember?> GetMembership(int workbenchId, string userId)
        {
            return await _workbenchData.GetMembership(workbenchId, userId);
        }

        public async Task<IEnumerable<WorkbenchMember>> GetMembers(int workbenchId)
        {
            return await _workbenchData.GetMembersForWorkbench(workbenchId);
        }

        public async Task<bool> InviteMember(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            if (!await _workbenchData.UserExists(userId)) return false;
            if (!await _workbenchData.WorkbenchExists(workbenchId)) return false;

            var member = new WorkbenchMember
            {
                WorkbenchId = workbenchId,
                UserId = userId,
                Role = role
            };

            return await _workbenchData.AddMember(member);
        }

        public async Task<bool> UpdateMemberRole(int workbenchId, string userId, WorkbenchMember.WorkbenchRole role)
        {
            return await _workbenchData.UpdateMemberRole(workbenchId, userId, role);
        }

        public async Task<bool> RemoveMember(int workbenchId, string userId)
        {
            return await _workbenchData.RemoveMember(workbenchId, userId);
        }
    }
}