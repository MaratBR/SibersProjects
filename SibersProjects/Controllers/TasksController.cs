using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.TaskService;
using SibersProjects.Services.TaskService.Exceptions;
using SibersProjects.Services.UsersService;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private readonly IProjectService _projectService;
    private readonly IUsersService _usersService;

    public TasksController(ITaskService taskService, IMapper mapper, AppDbContext dbContext, IProjectService projectService, IUsersService usersService)
    {
        _taskService = taskService;
        _mapper = mapper;
        _dbContext = dbContext;
        _projectService = projectService;
        _usersService = usersService;
    }

    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    public async Task<IActionResult> Create(NewTaskData data)
    {
        if (!User.IsInRole(RoleNames.ProjectManager) &&
            !await _projectService.IsAssignedToProject(User.GetUserId(), data.ProjectId))
        {
            return Forbid();
        }
        
        try
        {
            var task = await _taskService.Create(User.GetUserId(), data);

            return Created(Url.Action(nameof(GetTask), new { id = task.Id })!, new { task.Id });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public class GetTasksQuery : TaskFilterOptions
    {
        [Range(1, 1000)] public int Page { get; set; } = 1;
        [Range(1, 1000)] public int PageSize { get; set; } = 20;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDetailsDto>> GetTask(int id, [FromServices] IProjectService projectService)
    {
        var task = await _dbContext.Tasks.Where(t => t.Id == id)
            .ProjectTo<TaskDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (task == null)
            return NotFound($"Задача с идентификатором {id} не найдена");

        if (!User.IsInRole(RoleNames.Superuser))
        {
            var userId = User.GetUserId();
            if (!(User.IsInRole(RoleNames.ProjectManager) && await projectService.IsAssignedToProject(userId, task.Project.Id)) && task.Assignee?.Id != userId)
            {
                return Forbid();
            }
        }

        return task;
    }

    [Authorize(Roles = $"{RoleNames.ProjectManager}, {RoleNames.Superuser}")]
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskUpdateData body)
    {
        var task = await _dbContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
            return NotFound($"Задача с идентификатором {id} не найдена");

        if (!User.IsInRole(RoleNames.Superuser) && !await _projectService.IsAssignedToProject(User.GetUserId(), task.ProjectId))
        {
            return Forbid();
        }
        await _taskService.UpdateTask(task, body);
        return Ok();
    }
    

    public class UpdateStatus
    {
        [Required] public WorkTask.StatusEnum Status { get; set; }
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateTaskState(int id, UpdateStatus body)
    {
        var task = await _dbContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
            return NotFound($"Задача с идентификатором {id} не найдена");
        var userId = User.GetUserId();
        if (!User.IsInRole(RoleNames.Superuser) && 
            !(User.IsInRole(RoleNames.ProjectManager) && await _projectService.IsAssignedToProject(userId, task.ProjectId)) &&
            task.AssigneeId != userId)
            return Forbid();
        await _taskService.UpdateTaskState(task, body.Status);
        return Ok();
    }

    public class AssignToData
    {
        [Required] public string UserId { get; set; } = string.Empty;
    }

    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    [HttpPost("{id}/assignment")]
    public async Task<IActionResult> AssignToTask(int id, AssignToData data)
    {
        var task = await _dbContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
            return NotFound($"Задача с идентификатором {id} не найдена");
        if (!User.IsInRole(RoleNames.Superuser) &&
            !await _projectService.IsProjectManagerOf(User.GetUserId(), task.ProjectId))
        {
            return Forbid();
        }

        try
        {
            await _taskService.AssignTo(task, data.UserId);
        }
        catch (InvalidAssigneeException e)
        {
            return BadRequest(e.Message);
        }

        return Ok();
    }
    
    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    [HttpDelete("{id}/assignment")]
    public async Task<IActionResult> CancelTaskAssignment(int id)
    {
        var task = await _dbContext.Tasks.Where(t => t.Id == id).FirstOrDefaultAsync();
        if (task == null)
            return NotFound($"Задача с идентификатором {id} не найдена");
        if (!User.IsInRole(RoleNames.Superuser) &&
            !await _projectService.IsProjectManagerOf(User.GetUserId(), task.ProjectId))
        {
            return Forbid();
        }

        await _taskService.CancelTaskAssignment(task);

        return Ok();
    }


    [HttpGet("assigned")]
    public async Task<Pagination<TaskDto>> GetAssignedTasks([FromQuery] GetTasksQuery query)
    {
        var tasks = await _taskService.GetTaskQuery(query)
            .Skip(query.PageSize * (query.Page - 1))
            .Take(query.PageSize)
            .ProjectTo<TaskDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new Pagination<TaskDto>
        {
            Items = tasks,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
    
}