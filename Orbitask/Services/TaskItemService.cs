
using Orbitask.Models;
using Orbitask.Services.Interfaces;
using Orbitask.Data.Interfaces;

namespace Orbitask.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemData _taskData;

        public TaskItemService(ITaskItemData taskData)
        {
            _taskData = taskData;
        }


        // GET TASKS FOR COLUMN

        public async Task<IEnumerable<TaskItem>> GetTasksForColumn(int columnId)
        {
            return await _taskData.GetTasksForColumn(columnId);
        }


        // GET SINGLE TASK

        public async Task<TaskItem?> GetTask(int taskId)
        {
            return await _taskData.GetTask(taskId);
        }


        // CREATE TASK

        public async Task<TaskItem?> CreateTask(int columnId, TaskItem newTask)
        {
            // Validate column exists
            if (!await _taskData.ColumnExists(columnId))
                return null;

            // Get board for column
            var boardId = await _taskData.GetBoardIdForColumn(columnId);
            if (boardId == null)
                return null;

            // Fill required fields
            
            newTask.ColumnId = columnId;
            newTask.BoardId = boardId.Value;

            var createdTaskItem = await _taskData.InsertTask(newTask);
            return createdTaskItem;
        }


        // UPDATE TASK

        public async Task<TaskItem?> UpdateTask(int taskId, TaskItem updated)
        {
            // Check task exists
            if (!await _taskData.TaskExists(taskId))
                return null;

            // Ensure updated task keeps correct ID
            updated.Id = taskId;

            // Validate column exists
            if (!await _taskData.ColumnExists(updated.ColumnId))
                return null;

            // Validate board consistency
            var boardId = await _taskData.GetBoardIdForColumn(updated.ColumnId);
            if (boardId == null)
                return null;

            updated.BoardId = boardId.Value;

            var success = await _taskData.UpdateTask(updated);
            return success ? updated : null;
        }


        // DELETE TASK

        public async Task<bool> DeleteTask(int taskId)
        {
            return await _taskData.DeleteTask(taskId);
        }


        // ATTACH TAG

        public async Task<bool> AttachTag(int taskId, int tagId)
        {
            // Validate task exists
            if (!await _taskData.TaskExists(taskId))
                return false;

            // Validate tag exists
            if (!await _taskData.TagExists(tagId))
                return false;

            return await _taskData.AttachTag(taskId, tagId);
        }


        // REMOVE TAG

        public async Task<bool> RemoveTag(int taskId, int tagId)
        {
            // Validate task exists
            if (!await _taskData.TaskExists(taskId))
                return false;

            return await _taskData.RemoveTag(taskId, tagId);
        }
    }
}
