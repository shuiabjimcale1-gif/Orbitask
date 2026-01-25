using Orbitask.Models;

namespace Orbitask.Data.Interfaces
{
    public interface ITaskItemData
    {
        // ============================================
        // CORE CRUD OPERATIONS
        // ============================================

        /// <summary>
        /// Get a single task by ID
        /// </summary>
        Task<TaskItem?> GetTask(int taskId);

        /// <summary>
        /// Get all tasks for a specific column
        /// </summary>
        Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId);

        /// <summary>
        /// Insert a new task
        /// </summary>
        Task<TaskItem> InsertTask(TaskItem task);

        /// <summary>
        /// Update an existing task
        /// </summary>
        Task<bool> UpdateTask(TaskItem task);

        /// <summary>
        /// Delete a task and its tag associations
        /// </summary>
        Task<bool> DeleteTask(int taskId);

        // ============================================
        // TAG OPERATIONS (Many-to-Many)
        // ============================================

        /// <summary>
        /// Attach a tag to a task
        /// </summary>
        Task<bool> AttachTag(int taskId, int tagId);

        /// <summary>
        /// Remove a tag from a task
        /// </summary>
        Task<bool> RemoveTag(int taskId, int tagId);

        // ============================================
        // EXISTENCE CHECKS
        // ============================================

        /// <summary>
        /// Check if task exists
        /// </summary>
        Task<bool> TaskExists(int taskId);

        /// <summary>
        /// Check if column exists
        /// </summary>
        Task<bool> ColumnExists(int columnId);

        /// <summary>
        /// Check if tag exists
        /// </summary>
        Task<bool> TagExists(int tagId);

        // ============================================
        // TENANCY HELPER (NEW)
        // ============================================

        /// <summary>
        /// Get the WorkbenchId for a task (via JOINs)
        /// Used for authorization checks
        /// </summary>
        Task<int?> GetWorkbenchIdForTask(int taskId);
    }
}