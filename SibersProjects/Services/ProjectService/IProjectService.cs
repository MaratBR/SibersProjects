using SibersProjects.Dto;
using SibersProjects.Models;
using SibersProjects.Services.Common;
using SibersProjects.Utils;

namespace SibersProjects.Services.ProjectService;

public interface IProjectService
{
    /// <summary>
    ///     Метод фильтрует проекты по указанным критериям и возвращает объект IQueryable&lt;Project&gt;.
    /// </summary>
    /// <param name="filterOptions"></param>
    /// <returns>IQueryable с отфильтрованными проектами</returns>
    Task<Project> Create(NewProjectOptions options);

    Task<Project?> Get(int id);
    Task<bool> Exists(int id);
    Task<T?> GetAndProject<T>(int id) where T : class;
    Task Delete(Project project);

    Task<Pagination<ProjectListItemDto>> PaginateProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions);

    Task<Pagination<ProjectListItemDto>> PaginateAssignedProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions, User assignee);

    Task<Pagination<ProjectListItemDto>> PaginateManagedProjects(ProjectFilterOptions options,
        IPaginationOptions paginationOptions, User manager);

    Task<ProjectAssignment> AssignEmployee(User employee, Project project);
    Task CancelProjectAssignment(string employeeId, int projectId);
    Task<Project> Update(Project project, ProjectUpdateData data);
    Task<bool> IsAssignedToProject(string userId, int projectId);
    Task<bool> IsProjectManagerOf(string userId, int projectId);
}