using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.Common;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.ProjectService.Exceptions;
using SibersProjects.Services.TaskService;
using SibersProjects.Services.UsersService;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : Controller
{
    private readonly IMapper _mapper;
    private readonly IProjectService _projectService;
    private readonly UserManager<User> _userManager;
    private readonly IUsersService _usersService;

    public ProjectsController(
        IProjectService projectService,
        UserManager<User> userManager,
        IMapper mapper,
        IUsersService usersService)
    {
        _projectService = projectService;
        _userManager = userManager;
        _usersService = usersService;
        _mapper = mapper;
    }

    [HttpGet("assigned")]
    public async Task<Pagination<ProjectListItemDto>> GetAssignedProjects(
        [FromQuery] ProjectFilterOptions options,
        [FromQuery] DefaultPaginationOptions paginationOptions)
    {
        var user = await _usersService.RequireUser(User);
        return await _projectService.PaginateAssignedProjects(options, paginationOptions, user);
    }

    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.Superuser}")]
    [HttpGet("managed")]
    public async Task<Pagination<ProjectListItemDto>> GetManagingProjects(
        [FromQuery] ProjectFilterOptions options,
        [FromQuery] DefaultPaginationOptions paginationOptions)
    {
        var user = await _usersService.RequireUser(User);
        return await _projectService.PaginateManagedProjects(options, paginationOptions, user);
    }

    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    [HttpPost("{id}/assignments")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest request)
    {
        var project = await _projectService.Get(id);
        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        // только менежер проекта и суперпользоатель может назначить кого-то на проект
        if (!User.IsInRole(RoleNames.Superuser) && !await _projectService.IsProjectManagerOf(User.GetUserId(), id))
            return Forbid();
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return NotFound($"Пользователь {id} не найден");
        await _projectService.AssignEmployee(user, project);
        return Ok();
    }


    [HttpDelete("{projectId}/assignments/{userId}")]
    [Authorize(Roles = $"{RoleNames.Superuser}, {RoleNames.ProjectManager}")]
    public async Task<IActionResult> CancelAssignment(int projectId, string userId,
        [FromServices] ITaskService taskService)
    {
        if (!await _projectService.Exists(projectId))
            return NotFound($"Проект с идентификатором {projectId} не найден");
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound($"Пользователь с идентификатором {userId} не найден");
        if (!User.IsInRole(RoleNames.Superuser) &&
            !await _projectService.IsProjectManagerOf(User.GetUserId(), projectId)) return Forbid();

        await taskService.CancelAllTaskAssignmentsOnProject(projectId, userId);
        await _projectService.CancelProjectAssignment(userId, projectId);
        return Ok();
    }

    public class AssignRequest
    {
        [Required] public string UserId { get; set; } = string.Empty;
    }

    #region CRUD

    public class GetProjectsRequest : ProjectFilterOptions
    {
        // OFFSET может быть медленным, поэтому будем надеятся что никому не захочется глянуть 2001 страницу в списке проектов 
        [Range(1, 2000)] public int Page { get; set; } = 1;

        [Range(10, 50)] public int PageSize { get; set; } = 20;
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpGet]
    public async Task<ActionResult<Pagination<ProjectListItemDto>>> GetProjects(
        [FromQuery] ProjectFilterOptions options,
        [FromQuery] DefaultPaginationOptions paginationOptions)
    {
        return await _projectService.PaginateProjects(options, paginationOptions);
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpPost]
    public async Task<ActionResult<ProjectBaseDto>> CreateNewProject([FromBody] NewProjectOptions request)
    {
        try
        {
            var project = await _projectService.Create(request);
            return _mapper.Map<Project, ProjectBaseDto>(project);
        }
        catch (ProjectException e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProjectDetailsDto>> GetProject(int id)
    {
        var project = await _projectService.GetAndProject<ProjectDetailsDto>(id);

        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        if (!User.IsInRole(RoleNames.Superuser))
        {
            var userId = User.GetUserId();
            if (userId != project.ProjectManager?.Id && !await _projectService.IsAssignedToProject(userId, project.Id))
                return Forbid();
        }

        return project;
    }

    [Authorize(Roles = RoleNames.Superuser)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var project = await _projectService.Get(id);

        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        await _projectService.Delete(project);
        return Ok();
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = RoleNames.Superuser)]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdateData data)
    {
        var project = await _projectService.Get(id);

        if (project == null)
            return NotFound($"Проект с идентификатором {id} не найден");

        await _projectService.Update(project, data);
        return Ok();
    }

    #endregion
}