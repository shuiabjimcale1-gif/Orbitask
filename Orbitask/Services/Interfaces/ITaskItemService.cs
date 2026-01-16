using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface ITaskItemService
    {
        Task<bool> AttachTag(int taskId, int tagId);
        Task<TaskItem?> CreateTask(int columnId, TaskItem newTaskItem);
        Task<bool> DeleteTask(int taskId);
        Task<TaskItem?> GetTask(int taskId);
        Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId);
        Task<bool> RemoveTag(int taskId, int tagId);
        Task<TaskItem?> UpdateTask(int taskId, TaskItem updated);
    }
}