using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface ITaskItemData
    {
        // Core CRUD
        Task<TaskItem?> GetTask(int taskId);
        Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId);
        Task<TaskItem> InsertTask(TaskItem task);
        Task<bool> UpdateTask(TaskItem task);
        Task<bool> DeleteTask(int taskId);

        // Many-to-many
        Task<bool> AttachTag(int taskId, int tagId);
        Task<bool> RemoveTag(int taskId, int tagId);

        // Existence checks
        Task<bool> TaskExists(int taskId);
        Task<bool> ColumnExists(int columnId);
        Task<bool> TagExists(int tagId);
        Task<int?> GetBoardIdForColumn(int columnId);
    }

}