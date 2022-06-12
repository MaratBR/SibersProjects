using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.Common;
using SibersProjects.Services.ProjectService.Exceptions;
using SibersProjects.Utils;

namespace SibersProjects.Services.ProjectService;

public class ProjectServiceImpl : IProjectService
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProjectServiceImpl(AppDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Project> Create(NewProjectOptions options)
    {
        if (options.ProjectManagerId != null &&
            !await _dbContext.Users.Where(e => e.Id == options.ProjectManagerId).AnyAsync())
            throw new InvalidProjectManager(options.ProjectManagerId);

        if (options.StartsAt > options.EndsAt)
            throw new InvalidProjectTimeSpan(options.StartsAt, options.EndsAt, "дата начала позже даты конца");

        var project = new Project
        {
            Name = options.Name.Trim(),
            ClientCompany = options.ClientCompany.Trim(),
            ContractorCompany = options.ContractorCompany.Trim(),
            ProjectManagerId = options.ProjectManagerId,
            Priority = options.Priority,
            StartsAt = options.StartsAt,
            EndsAt = options.EndsAt,
            Description = options.Description
        };
        _dbContext.Add(project);
        await _dbContext.SaveChangesAsync();

        return project;
    }

    public Task<Project?> Get(int id)
    {
        return _dbContext.Projects.Where(p => p.Id == id).FirstOrDefaultAsync();
    }

    public Task<bool> Exists(int id)
    {
        return _dbContext.Projects.Where(p => p.Id == id).AnyAsync();
    }

    public Task<T?> GetAndProject<T>(int id) where T : class
    {
        return _dbContext.Projects.Where(p => p.Id == id).ProjectTo<T>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task Delete(Project project)
    {
        _dbContext.Remove(project);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Pagination<ProjectListItemDto>> PaginateProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions)
    {
        return new Pagination<ProjectListItemDto>
        {
            Items = await GetProjectsQuery(options)
                .Skip((paginationOptions.Page - 1) * paginationOptions.PageSize)
                .Take(paginationOptions.PageSize)
                .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(),
            Page = paginationOptions.Page,
            PageSize = paginationOptions.PageSize
        };
    }

    public async Task<Pagination<ProjectListItemDto>> PaginateAssignedProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions, User assignee)
    {
        return new Pagination<ProjectListItemDto>
        {
            Items = await GetProjectsQuery(options)
                .Where(p => p.Employees.Any(u => u.Id == assignee.Id))
                .Skip((paginationOptions.Page - 1) * paginationOptions.PageSize)
                .Take(paginationOptions.PageSize)
                .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(),
            Page = paginationOptions.Page,
            PageSize = paginationOptions.PageSize
        };
    }

    public async Task<Pagination<ProjectListItemDto>> PaginateManagedProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions, User manager)
    {
        return new Pagination<ProjectListItemDto>
        {
            Items = await GetProjectsQuery(options)
                .Where(p => p.ProjectManagerId == manager.Id)
                .Skip((paginationOptions.Page - 1) * paginationOptions.PageSize)
                .Take(paginationOptions.PageSize)
                .ProjectTo<ProjectListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync(),
            Page = paginationOptions.Page,
            PageSize = paginationOptions.PageSize
        };
    }

    public async Task<ProjectAssignment> AssignEmployee(User employee, Project project)
    {
        var assignment = await _dbContext.Assignments
            .Where(a => a.EmployeeId == employee.Id && a.ProjectId == project.Id).FirstOrDefaultAsync();
        if (assignment != null)
            return assignment;
        assignment = new ProjectAssignment { EmployeeId = employee.Id, ProjectId = project.Id };
        _dbContext.Add(assignment);
        await _dbContext.SaveChangesAsync();
        return assignment;
    }

    public async Task CancelProjectAssignment(string employeeId, int projectId)
    {
        var assignment =
            await _dbContext.Assignments.Where(a => a.ProjectId == projectId && a.EmployeeId == employeeId)
                .FirstOrDefaultAsync();

        if (assignment == null)
            // TODO: throw
            return;

        _dbContext.Remove(assignment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Project> Update(Project project, ProjectUpdateData data)
    {
        if (data.Name != null)
            project.Name = data.Name;
        if (data.Priority != null)
            project.Priority = (int)data.Priority;
        if (data.ClientCompany != null)
            project.ClientCompany = data.ClientCompany;
        if (data.ContractorCompany != null)
            project.ContractorCompany = data.ContractorCompany;
        if (data.EndsAt != null)
            project.EndsAt = (DateTime)data.EndsAt;
        if (data.StartsAt != null)
            project.StartsAt = (DateTime)data.StartsAt;

        if (project.StartsAt > project.EndsAt)
        {
            throw new InvalidProjectTimeSpan(project.StartsAt, project.EndsAt, "дата начала позже даты конца");
        }
        
        if (data.ProjectManagerId != null)
            project.ProjectManagerId = data.ProjectManagerId;
        await _dbContext.SaveChangesAsync();
        return project;
    }

    public Task<bool> IsAssignedToProject(string userId, int projectId)
    {
        return _dbContext.Assignments.Where(a => a.EmployeeId == userId && a.ProjectId == projectId).AnyAsync();
    }

    public Task<bool> IsProjectManagerOf(string userId, int projectId)
    {
        return _dbContext.Projects.Where(p => p.Id == projectId && p.ProjectManagerId == userId).AnyAsync();
    }

    #region Utils

    private IQueryable<Project> GetProjectsQuery(ProjectFilterOptions filterOptions)
    {
        IQueryable<Project> query = _dbContext.Projects;

        if (filterOptions.StartsAfter != null)
            query = query.Where(p => p.StartsAt >= filterOptions.StartsAfter);
        if (filterOptions.EndsBefore != null)
            query = query.Where(p => p.EndsAt <= filterOptions.EndsBefore);
        if (filterOptions.ProjectManagers != null && filterOptions.ProjectManagers.Count > 0)
            query = query.Where(p =>
                p.ProjectManagerId != null && filterOptions.ProjectManagers.Contains(p.ProjectManagerId));

        // TODO: NormalizedContractorCompany, NormalizedClientCompany
        if (filterOptions.Contractor != null)
            query = query.Where(p => p.ContractorCompany == filterOptions.Contractor);
        if (filterOptions.Client != null)
            query = query.Where(p => p.ClientCompany == filterOptions.Client);

        switch (filterOptions.SortBy)
        {
            case ProjectFilterOptions.SortByEnum.Name:
                query = query.OrderBy(p => p.Name);
                break;
            case ProjectFilterOptions.SortByEnum.ProjectManager:
                query = query.OrderBy(p => p.ProjectManagerId);
                break;
            case ProjectFilterOptions.SortByEnum.StartsAt:
                query = query.OrderBy(p => p.StartsAt);
                break;
            case ProjectFilterOptions.SortByEnum.EndsAt:
                query = query.OrderBy(p => p.EndsAt);
                break;
            case ProjectFilterOptions.SortByEnum.Priority:
                query = query.OrderByDescending(p => p.Priority);
                break;
            case ProjectFilterOptions.SortByEnum.Client:
                query = query.OrderBy(p => p.ClientCompany);
                break;
            case ProjectFilterOptions.SortByEnum.Contractor:
                query = query.OrderBy(p => p.ContractorCompany);
                break;
        }

        return query;
    }

    #endregion
}