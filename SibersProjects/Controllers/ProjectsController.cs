using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.ProjectService.Exceptions;
using SibersProjects.Services.TaskService;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public ProjectsController(IProjectService projectService, AppDbContext dbContext, IMapper mapper, UserManager<User> userManager)
    {
        _dbContext = dbContext;
        _projectService = projectService;
        _mapper = mapper;
        _userManager = userManager;
    }

    #region CRUD
    
    public class GetProjectsRequest : ProjectFilterOptions
    {
        // OFFSET может быть медленным, поэтому будем надеятся что никому не захочется глянуть 2001 страницу в списке проектов 
        [Range(1, 2000)]
        public int Page { get; set; } = 1;

        [Range(10, 50)] public int PageSize { get; set; } = 20;
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpGet]
    public async Task<ActionResult<PaginationResponse<ProjectListItemDto>>> GetProjects([FromQuery] GetProjectsRequest options)
    {
        var projects = await _projectService.GetProjectsQuery(options)
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResponse<ProjectListItemDto>
        {
            Page = options.Page,
            PageSize = options.PageSize,
            Items = projects
        };
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpPost]
    public async Task<ActionResult<ProjectBaseDto>> CreateNewProject([FromBody] NewProjectOptions request)
    {
        try
        {
            var project = await _projectService.CreateProject(request);
            return _mapper.Map<Project, ProjectBaseDto>(project);
        }
        catch (ProjectException e)
        {
            // TODO: более конкретно
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
    {
        var project = await _dbContext.Projects
            .Where(p => p.Id == id)
            .ProjectTo<ProjectDetailsDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");
        
        if (!User.IsInRole(RoleNames.Superuser))
        {
            var userId = User.GetUserId();
            if (userId != project.ProjectManager?.Id && !await _projectService.IsAssignedToProject(userId, project.Id))
            {
                return Forbid();
            }
        }

        return project;
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _dbContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();
        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        _dbContext.Remove(project);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Superuser)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdateData data)
    {
        var project = await _dbContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();
        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        await _projectService.Update(project, data);
        // TODO: map to ProjectDto
        return Ok();
    }

    #endregion
    
    [HttpGet("assigned")]
    public async Task<PaginationResponse<ProjectListItemDto>> GetAssignedProjects([FromQuery] GetProjectsRequest request)
    {
        var userId = User.GetUserId();
        var assignedProjects = await _projectService
            .GetProjectsQuery(request)
            .Where(p => p.Employees.Any(e => e.Id == userId))
            .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResponse<ProjectListItemDto>
        {
            Items = assignedProjects,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.Superuser}")]
    [HttpGet("managed")]
    public async Task<PaginationResponse<ProjectListItemDto>> GetManagingProjects([FromQuery] GetProjectsRequest request)
    {
        var userId = User.GetUserId();
        var projects = await _projectService
            .GetProjectsQuery(request)
            .Where(p => p.ProjectManagerId == userId)
            .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PaginationResponse<ProjectListItemDto>
        {
            Items = projects,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public class AssignRequest
    {
        [Required] public string UserId { get; set; } = string.Empty;
    }

    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    [HttpPost("{id}/assignments")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest request)
    {
        var project = await _dbContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();
        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        // только менежер проекта и суперпользоатель может назначить кого-то на проект
        if (!User.IsInRole(RoleNames.Superuser) && !await _projectService.IsProjectManagerOf(User.GetUserId(), id))
        {
            return Forbid();
        }
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return NotFound($"Пользователь {id} не найден");
        await _projectService.AssignEmployee(user, project);
        return Ok();
    }


    [HttpDelete("{projectId}/assignments/{userId}")]
    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    public async Task<IActionResult> CancelAssignment(int projectId, string userId, [FromServices] ITaskService taskService)
    {
        if (!await _dbContext.Projects.Where(p => p.Id == projectId).AnyAsync())
        {
            return NotFound($"Проект с идентификатором {projectId} не найден");
        }
        if (!await _dbContext.Users.Where(u => u.Id == userId).AnyAsync())
        {
            return NotFound($"Пользователь с идентификатором {userId} не найден");
        }
        if (!User.IsInRole(RoleNames.Superuser) && !await _projectService.IsProjectManagerOf(User.GetUserId(), projectId))
        {
            return Forbid();
        }

        await _projectService.CancelProjectAssignment(userId, projectId);
        return Ok();
    }
}