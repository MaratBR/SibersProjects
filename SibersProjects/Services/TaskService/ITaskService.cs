using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.Common;
using SibersProjects.Utils;

namespace SibersProjects.Services.TaskService;

public interface ITaskService
{
    Task<WorkTask> Create(string authorId, NewTaskData data);
    Task<WorkTask?> Get(int id);
    Task<T?> GetAndProject<T>(int id);
    Task<bool> IsAssignedTo(string userId, int taskId);

    /// <summary>
    ///     Назначает сотрудника на задачу
    /// </summary>
    /// <param name="task">Задача</param>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <exception cref="Exceptions.InvalidAssigneeException">Если пользователь не может быть назначен на задачу</exception>
    Task AssignTo(WorkTask task, string userId);

    Task UpdateTaskState(WorkTask task, WorkTask.StatusEnum newStatus);
    Task UpdateTask(WorkTask task, TaskUpdateData data);
    Task CancelAllTaskAssignmentsOnProject(int projectId, string userId);
    Task CancelTaskAssignment(WorkTask task);

    Task<Pagination<TaskDto>> PaginateTasks(TaskFilterOptions filterOptions,
        DefaultPaginationOptions paginationOptions);
}