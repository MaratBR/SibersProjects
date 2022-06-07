using System.ComponentModel.DataAnnotations;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService;
using SibersProjects.Services.ProjectService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Controllers;

[ApiController]
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

    public class ProjectView
    {
        public class ManagerView
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
        }

        public Project Project { get; set; } = null!;
        public ManagerView Manager { get; set; } = null!;
    }

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

    public class NewProjectResponse
    {
        public Project Project { get; set; } = null!;
    }

    [HttpPost]
    public async Task<ActionResult<NewProjectResponse>> CreateNewProject([FromBody] NewProjectOptions request)
    {
        try
        {
            var project = await _projectService.CreateProject(request);
            return new NewProjectResponse { Project = project };
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

        return project;
    }

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

    public class AssignRequest
    {
        [Required] public string UserId { get; set; } = string.Empty;
    }

    [HttpPost("{id}/assignments")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return NotFound($"Пользователь {id} не найден");
        var project = await _dbContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();
        await _projectService.AssignEmployee(user, project);
        return Ok();
    }


    [HttpDelete("{id}/assignments/{userId}")]
    public async Task<IActionResult> CancelAssignment(int id, string userId)
    {
        if (!await _dbContext.Projects.Where(p => p.Id == id).AnyAsync())
        {
            return NotFound($"Проект с идентификатором {id} не найден");
        }
        if (!await _dbContext.Users.Where(u => u.Id == userId).AnyAsync())
        {
            return NotFound($"Пользователь с идентификатором {userId} не найден");
        }
        await _projectService.CancelProjectAssignment(userId, id);
        return Ok();
    }
}