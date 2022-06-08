using Microsoft.EntityFrameworkCore;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TaskService.Exceptions;

namespace SibersProjects.Services.TaskService;

public class TaskServiceImpl : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IProjectService _projectService;
    
    public TaskServiceImpl(AppDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
    }
    
    public IQueryable<WorkTask> GetTaskQuery(TaskFilterOptions options)
    {
        IQueryable<WorkTask> queryable = _context.Tasks;

        if (options.Status != null)
            queryable = queryable.Where(t => t.Status == options.Status);

        switch (options.SortBy)
        {
            case TaskFilterOptions.SortByEnum.Priority:
                queryable = queryable.OrderByDescending(t => t.Priority);
                break;
            case TaskFilterOptions.SortByEnum.NewestToOldest:
                queryable = queryable.OrderByDescending(t => t.CreatedAt);
                break;
            case TaskFilterOptions.SortByEnum.OldestToNewest:
                queryable = queryable.OrderBy(t => t.CreatedAt);
                break;
        }

        return queryable;
    }

    public Task<bool> IsAssignedTo(string userId, int taskId)
    {
        return _context.Tasks.Where(t => t.Id == taskId && t.AssigneeId == userId).AnyAsync();
    }

    public async Task AssignTo(string userId, int taskId)
    {
        var task = await _context.Tasks.Where(t => t.Id == taskId).FirstOrDefaultAsync();
        if (task == null)
        {
            return;
        }
        if (task.AssigneeId == userId)
            return;
        task.AssigneeId = userId;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskState(WorkTask task, WorkTask.StatusEnum newStatus)
    {
        if (task.Status == newStatus)
            return;
        task.Status = newStatus;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTask(WorkTask task, TaskUpdateData data)
    {
        if (data.Description != null)
            task.Description = data.Description;
        if (data.Name != null)
            task.Name = data.Name;
        if (data.Status != null)
            task.Status = (WorkTask.StatusEnum)data.Status;
        await _context.SaveChangesAsync();
    }

    public async Task CancelAllTaskAssignmentsOnProject(int projectId, string userId)
    {
        var tasks = await _context.Tasks.Where(t => t.ProjectId == projectId && t.AssigneeId == userId).ToListAsync();

        foreach (var task in tasks)
        {
            task.AssigneeId = null;
        }

        await _context.SaveChangesAsync();
    }

    public async Task CancelTaskAssignment(WorkTask task)
    {
        task.AssigneeId = null;
        await _context.SaveChangesAsync();
    }
}