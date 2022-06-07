using Microsoft.EntityFrameworkCore;
using SibersProjects.Models;
using SibersProjects.Services.ProjectService.Exceptions;

namespace SibersProjects.Services.ProjectService;

public class ProjectServiceImpl : IProjectService
{
    private readonly AppDbContext _dbContext;
    
    public ProjectServiceImpl(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<Project> GetProjectsQuery(ProjectFilterOptions filterOptions)
    {
        IQueryable<Project> query = _dbContext.Projects;

        if (filterOptions.StartsAfter != null)
            query = query.Where(p => p.StartsAt >= filterOptions.StartsAfter);
        if (filterOptions.EndsBefore != null)
            query = query.Where(p => p.EndsAt <= filterOptions.EndsBefore);
        if (filterOptions.ProjectManagers != null && filterOptions.ProjectManagers.Count > 0)
            query = query.Where(p => filterOptions.ProjectManagers.Contains(p.ProjectManagerId));

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
        }

        return query;
    }

    public async Task<Project> CreateProject(NewProjectOptions options)
    {
        if (!await _dbContext.Users.Where(e => e.Id == options.ProjectManagerId).AnyAsync())
        {
            throw new InvalidProjectManager(options.ProjectManagerId);
        }
        
        if (options.StartsAt > options.EndsAt)
        {
            throw new InvalidProjectTimeSpan(options.StartsAt, options.EndsAt, "дата начала позже даты конца");
        }

        var project = new Project
        {
            Name = options.Name.Trim(),
            ClientCompany = options.ClientCompany.Trim(),
            ContractorCompany = options.ContractorCompany.Trim(),
            ProjectManagerId = options.ProjectManagerId,
            Priority = options.Priority,
            StartsAt = options.StartsAt,
            EndsAt = options.EndsAt
        };
        _dbContext.Add(project);
        await _dbContext.SaveChangesAsync();

        return project;
    }

    public async Task<ProjectAssignment> AssignEmployee(User employee, Project project)
    {
        // TODO: exception try/catch
        var assignment = new ProjectAssignment { Employee = employee, Project = project };
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
        {
            // TODO: throw
            return;
        }

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
        if (data.ProjectManagerId != null)
            project.ProjectManagerId = data.ProjectManagerId;
        await _dbContext.SaveChangesAsync();
        return project;
    }
}