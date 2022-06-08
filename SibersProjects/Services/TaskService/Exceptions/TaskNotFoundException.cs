namespace SibersProjects.Services.TaskService.Exceptions;

public class TaskNotFoundException : TaskException
{
    public TaskNotFoundException(int id) : base($"Задача с идентификатором {id} не найдена")
    {
    }
}