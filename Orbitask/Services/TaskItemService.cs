using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemData _taskData;
        private readonly IColumnData _columnData;
        private readonly ITagData _tagData;

        public TaskItemService(
            ITaskItemData taskData,
            IColumnData columnData,
            ITagData tagData)
        {
            _taskData = taskData;
            _columnData = columnData;
            _tagData = tagData;
        }

        // ============================================
        // GET SINGLE TASK
        // ============================================

        public async Task<TaskItem?> GetTask(int taskId)
        {
            return await _taskData.GetTask(taskId);
        }

        // ============================================
        // GET TASKS FOR COLUMN
        // ============================================

        public async Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId)
        {
            return await _taskData.GetTasksForColumn(columnId);
        }

        // ============================================
        // CREATE TASK
        // ============================================

        public async Task<TaskItem?> CreateTask(int columnId, TaskItem newTask)
        {
            // 1. Validate column exists
            var column = await _columnData.GetColumn(columnId);
            if (column == null)
                return null;

            // 2. Set only direct parent FK
            newTask.ColumnId = columnId;

            // ❌ REMOVED: newTask.BoardId = board.Id;
            // ❌ REMOVED: newTask.WorkbenchId = board.WorkbenchId;

            // BoardId and WorkbenchId are now derived via JOINs, not stored

            // 3. Insert task
            return await _taskData.InsertTask(newTask);
        }

        // ============================================
        // UPDATE TASK
        // ============================================

        public async Task<TaskItem?> UpdateTask(int taskId, TaskItem updated)
        {
            // 1. Load the existing task (ensures it exists)
            var existing = await _taskData.GetTask(taskId);
            if (existing == null)
                return null;

            // 2. Validate new column exists
            var newColumn = await _columnData.GetColumn(updated.ColumnId);
            if (newColumn == null)
                return null;

            // 3. 🔒 SECURITY: Prevent moving task to different board
            if (updated.ColumnId != existing.ColumnId)
            {
                // Get the existing column to check its board
                var existingColumn = await _columnData.GetColumn(existing.ColumnId);
                if (existingColumn == null)
                    return null;

                // Ensure both columns are on the same board
                if (newColumn.BoardId != existingColumn.BoardId)
                {
                    throw new InvalidOperationException(
                        $"Cannot move task from Board {existingColumn.BoardId} to Board {newColumn.BoardId}. " +
                        "Tasks cannot be moved between boards."
                    );
                }
            }

            // 4. Apply required IDs
            updated.Id = taskId;
            updated.ColumnId = newColumn.Id;

            // 5. Update
            var success = await _taskData.UpdateTask(updated);s
            return success ? updated : null;
        }

        // ============================================
        // DELETE TASK
        // ============================================

        public async Task<bool> DeleteTask(int taskId)
        {
            // Validate task exists
            if (!await _taskData.TaskExists(taskId))
                return false;

            return await _taskData.DeleteTask(taskId);
        }

        // ============================================
        // ATTACH TAG TO TASK
        // ============================================

        public async Task<bool> AttachTag(int taskId, int tagId)
        {
            // 1. Validate task exists
            if (!await _taskData.TaskExists(taskId))
                return false;

            // 2. Validate tag exists
            if (!await _taskData.TagExists(tagId))
                return false;

            // 3. 🔒 SECURITY: Ensure tag and task are on the same board
            var task = await _taskData.GetTask(taskId);
            var tag = await _tagData.GetTag(tagId);

            if (task == null || tag == null)
                return false;

            // Get the column to find the board
            var column = await _columnData.GetColumn(task.ColumnId);
            if (column == null)
                return false;

            // Verify tag and task are on the same board
            if (tag.BoardId != column.BoardId)
            {
                throw new InvalidOperationException(
                    $"Cannot attach tag from Board {tag.BoardId} to task on Board {column.BoardId}. " +
                    "Tags can only be attached to tasks on the same board."
                );
            }

            // 4. Attach tag
            return await _taskData.AttachTag(taskId, tagId);
        }

        // ============================================
        // REMOVE TAG FROM TASK
        // ============================================

        public async Task<bool> RemoveTag(int taskId, int tagId)
        {
            if (!await _taskData.TaskExists(taskId))
                return false;

            return await _taskData.RemoveTag(taskId, tagId);
        }
    }
}