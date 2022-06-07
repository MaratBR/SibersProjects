using SibersProjects.Models;

namespace SibersProjects.Services.ProjectService;

public interface IProjectService
{
    /// <summary>
    /// Метод фильтрует проекты по указанным критериям и возвращает объект IQueryable&lt;Project&gt;.
    /// </summary>
    /// <param name="filterOptions"></param>
    /// <returns>IQueryable с отфильтрованными проектами</returns>
    IQueryable<Project> GetProjectsQuery(ProjectFilterOptions filterOptions);
    Task<Project> CreateProject(NewProjectOptions options);
    Task<ProjectAssignment> AssignEmployee(User employee, Project project);
    Task CancelProjectAssignment(string employeeId, int projectId);
    Task<Project> Update(Project project, ProjectUpdateData data);
}

