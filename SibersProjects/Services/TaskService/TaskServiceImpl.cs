using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.Common;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TaskService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Services.TaskService;

public class TaskServiceImpl : ITaskService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IProjectService _projectService;

    public TaskServiceImpl(AppDbContext context, IProjectService projectService, IMapper mapper)
    {
        _context = context;
        _projectService = projectService;
        _mapper = mapper;
    }

    public async Task<WorkTask> Create(string authorId, NewTaskData data)
    {
        if (!await _context.Projects.Where(p => p.Id == data.ProjectId).AnyAsync())
            throw new TaskException($"Проект с идентификатором {data.ProjectId} не найден");
        if (!await _context.Users.Where(u => u.Id == authorId).AnyAsync())
            throw new TaskException($"Пользователь с идентификатором {authorId} не найден");
        var task = new WorkTask
        {
            ProjectId = data.ProjectId,
            Name = data.Name,
            Description = data.Description,
            AuthorId = authorId,
            Priority = data.Priority
        };
        _context.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public Task<WorkTask?> Get(int id)
    {
        return _context.Tasks.Where(t => t.Id == id).SingleOrDefaultAsync();
    }

    public Task<T?> GetAndProject<T>(int id)
    {
        return _context.Tasks.Where(t => t.Id == id)
            .ProjectTo<T>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }

    public Task<bool> IsAssignedTo(string userId, int taskId)
    {
        return _context.Tasks.Where(t => t.Id == taskId && t.AssigneeId == userId).AnyAsync();
    }

    public async Task AssignTo(WorkTask task, string userId)
    {
        if (!await _projectService.IsAssignedToProject(userId, task.ProjectId))
            throw new InvalidAssigneeException(userId, "сотрудник не назначен на проект");
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

        foreach (var task in tasks) task.AssigneeId = null;

        await _context.SaveChangesAsync();
    }

    public async Task CancelTaskAssignment(WorkTask task)
    {
        task.AssigneeId = null;
        await _context.SaveChangesAsync();
    }

    public async Task<Pagination<TaskDto>> PaginateTasks(TaskFilterOptions filterOptions,
        DefaultPaginationOptions paginationOptions)
    {
        return new Pagination<TaskDto>
        {
            Items = await GetTaskQuery(filterOptions)
                .Skip((paginationOptions.Page - 1) * paginationOptions.PageSize)
                .Take(paginationOptions.PageSize)
                .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
                .ToListAsync(),
            Page = paginationOptions.Page,
            PageSize = paginationOptions.PageSize
        };
    }

    private IQueryable<WorkTask> GetTaskQuery(TaskFilterOptions options)
    {
        IQueryable<WorkTask> queryable = _context.Tasks;

        if (options.Status != null)
            queryable = queryable.Where(t => t.Status == options.Status);
        if (options.ProjectId == null)
            queryable = queryable.Where(t => t.ProjectId == options.ProjectId);

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
}