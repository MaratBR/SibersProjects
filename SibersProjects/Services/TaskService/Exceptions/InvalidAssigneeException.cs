namespace SibersProjects.Services.TaskService.Exceptions;

public class InvalidAssigneeException : TaskException
{
    public InvalidAssigneeException(string assigneeId, string error) : base(
        $"Нельзя назначить сотрудника ID={assigneeId} на задачу: {error}")
    {
    }
}