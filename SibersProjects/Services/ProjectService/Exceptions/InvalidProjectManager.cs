namespace SibersProjects.Services.ProjectService.Exceptions;

public class InvalidProjectManager : ProjectException
{
    public InvalidProjectManager(string projectManagerId) : base($"Менеджер проекта с идентификатором {projectManagerId} не найден")
    {
        
    }
}