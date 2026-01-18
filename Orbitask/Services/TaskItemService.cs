
using Orbitask.Data;
using Orbitask.Data.Interfaces;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly ITaskItemData _taskData;
        private readonly IColumnData _columnData;
        private readonly IBoardData _boardData;
        public TaskItemService(ITaskItemData taskData, IColumnData columnData, IBoardData boardData)
        {
            _taskData = taskData;
            _columnData = columnData;
            _boardData = boardData;
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
            var column = await _columnData.GetColumn(columnId);
            if (column == null)
                return null;
            var board = await _boardData.GetBoard(column.BoardId);
            if (board == null)
                return null;
            newTask.ColumnId = columnId;
            newTask.BoardId = board.Id;
            newTask.WorkbenchId = board.WorkbenchId;

            return await _taskData.InsertTask(newTask);
        }



        // UPDATE TASK

        public async Task<TaskItem?> UpdateTask(int taskId, TaskItem updated)
        {
            // Load the existing task (ensures it exists)
            var existing = await _taskData.GetTask(taskId);
            if (existing == null)
                return null;

            // Load the column (we need its BoardId)
            var column = await _columnData.GetColumn(updated.ColumnId);
            if (column == null)
                return null;

            // Load the board (we need its WorkbenchId)
            var board = await _boardData.GetBoard(column.BoardId);
            if (board == null)
                return null;

            // Apply required IDs
            updated.Id = taskId;
            updated.ColumnId = column.Id;
            updated.BoardId = board.Id;
            updated.WorkbenchId = board.WorkbenchId;

            // Update
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
