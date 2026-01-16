using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface IWorkbenchService
    {
        Task<Workbench?> GetWorkbench(int id);
        Task<IEnumerable<Workbench>> GetWorkbenchesForUser(string userId);

        Task<Workbench?> CreateWorkbench(string userId, Workbench workbench);
        Task<Workbench?> UpdateWorkbench(int id, Workbench updated);
        Task<bool> DeleteWorkbench(int id, string userId);
    }

}
