using Orbitask.Models;

namespace Orbitask.Services.Interfaces
{
    public interface ITaskItemService
    {
        /// <summary>
        /// Get a single task by ID
        /// </summary>
        Task<TaskItem?> GetTask(int taskId);

        /// <summary>
        /// Get all tasks for a specific column
        /// </summary>
        Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId);

        /// <summary>
        /// Create a new task in a column
        /// </summary>
        Task<TaskItem?> CreateTask(int columnId, TaskItem newTask);

        /// <summary>
        /// Update an existing task
        /// </summary>
        Task<TaskItem?> UpdateTask(int taskId, TaskItem updated);

        /// <summary>
        /// Delete a task
        /// </summary>
        Task<bool> DeleteTask(int taskId);

        /// <summary>
        /// Attach a tag to a task
        /// </summary>
        Task<bool> AttachTag(int taskId, int tagId);

        /// <summary>
        /// Remove a tag from a task
        /// </summary>
        Task<bool> RemoveTag(int taskId, int tagId);
    }
}