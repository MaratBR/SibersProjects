using SibersProjects.Dto;
using SibersProjects.Models;

namespace SibersProjects.Services.TaskService;

public interface ITaskService
{
    IQueryable<WorkTask> GetTaskQuery(TaskFilterOptions options);
    Task<bool> IsAssignedTo(string userId, int taskId);
    Task AssignTo(string userId, int taskId);
    Task UpdateTaskState(WorkTask task, WorkTask.StatusEnum newStatus);
    Task UpdateTask(WorkTask task, TaskUpdateData data);
    Task CancelAllTaskAssignmentsOnProject(int projectId, string userId);
    Task CancelTaskAssignment(WorkTask task);
}